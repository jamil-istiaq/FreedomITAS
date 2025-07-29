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
        private readonly string _apiKey;
        private readonly string _apiBaseUrl;

        public DreamscapeService(IHttpClientFactory httpClientFactory, IOptions<DreamscapeSettings> settings)
        {
            _httpClientFactory = httpClientFactory;
            _apiKey = settings.Value.ApiKey;
            _apiBaseUrl = settings.Value.ApiBaseUrl;
        }

        public async Task<HttpResponseMessage> CreateCompanyAsync(object payload)
        {
            var client = _httpClientFactory.CreateClient();

            // 1. Generate request ID and signature
            string guid = Guid.NewGuid().ToString();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string requestIdRaw = guid + timestamp;
            string requestId = GenerateMd5(requestIdRaw);
            string signature = GenerateMd5(requestId + _apiKey);

            // 2. Add required headers
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}customers");
            request.Headers.Add("api-request-id", requestId);
            request.Headers.Add("api-signature", signature);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


            // 3. Serialize payload
            var json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            // 4. Send POST request
            var response = await client.SendAsync(request);
            
            return response;
        }

        private static string GenerateMd5(string input)
        {
            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
