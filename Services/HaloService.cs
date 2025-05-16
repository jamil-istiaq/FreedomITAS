using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

public class HaloServices
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    private string _accessToken;

    public HaloServices(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken))
            return _accessToken;

        var tokenUrl = _configuration["HaloPSA:TokenUrl"];
        var clientId = _configuration["HaloPSA:ClientId"];
        var clientSecret = _configuration["HaloPSA:ClientSecret"];

        var requestBody = new StringContent(
            $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}",
            Encoding.UTF8, "application/x-www-form-urlencoded"
        );

        var response = await _httpClient.PostAsync(tokenUrl, requestBody);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);
        _accessToken = doc.RootElement.GetProperty("access_token").GetString();

        return _accessToken;
    }

    public async Task<string> GetClientsAsync()
    {
        var token = await GetAccessTokenAsync();
        var apiUrl = _configuration["HaloPSA:ApiBaseUrl"] + "/Clients";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
