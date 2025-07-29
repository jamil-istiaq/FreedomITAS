using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace FreedomITAS.API_Serv
{
    public class DreamscapeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly DreamscapeSettings _settings;
        
        public DreamscapeService(IHttpClientFactory httpClientFactory, IOptions<DreamscapeSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _apiKey = _settings.ApiKey;
        }

        public async Task<string> CallApiAsync(string endpoint, HttpMethod method, object payload = null)
        {
            var client = _httpClientFactory.CreateClient();
           
            string requestId = GenerateMd5($"{Guid.NewGuid()}{DateTime.UtcNow.Ticks}");
            string signature = GenerateMd5(requestId + _apiKey);

            var request = new HttpRequestMessage(method, $"{_settings.ApiBaseUrl}{endpoint}");
            request.Headers.Add("Api-Request-Id", requestId);
            request.Headers.Add("Api-Signature", signature);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (payload != null)
            {
                string json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"API Error: {response.StatusCode} - {error}");
            }

            return await response.Content.ReadAsStringAsync();
        }


        private static string GenerateMd5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }


        public async Task<HttpResponseMessage> CreateCompanyAsync(object payload)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Generate request ID and signature
            string requestId = GenerateMd5($"{Guid.NewGuid()}{DateTime.UtcNow.Ticks}");
            string signature = GenerateMd5(requestId + _apiKey);

            // 2. Add required headers
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("Api-Request-Id", requestId);
            client.DefaultRequestHeaders.Add("Api-Signature", signature);

            // 3. Serialize payload
            var content = new StringContent(JsonSerializer.Serialize(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // 4. Send POST request
            var response = await client.PostAsync($"{_settings.ApiBaseUrl}customers", content);

            // 5. Optional: Throw exception on error
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApplicationException($"Dreamscape API Error: {response.StatusCode} - {errorContent}");
            }

            return response;
        }
    }
}
