using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FreedomITAS
{
    public class HaloPSAService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HaloPSA _settings;

        public HaloPSAService(IHttpClientFactory httpClientFactory, IOptions<HaloPSA> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var form = new Dictionary<string, string>
        {
            {"grant_type", "client_credentials"},
            {"client_id", _settings.ClientId},
            {"client_secret", _settings.ClientSecret}
        };

            var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<string> GetTicketsAsync()
        {
            var token = await GetAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_settings.ApiBaseUrl}/tickets");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
