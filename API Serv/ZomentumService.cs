using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;

namespace FreedomITAS.API_Serv
{
    public class ZomentumService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ZomentumSettings _settings;

        public ZomentumService(IHttpClientFactory httpClientFactory, IOptions<ZomentumSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        public async Task<string> GetLeadsAsync()
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"{_settings.ApiBaseUrl}leads");

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<JsonElement> GetClientsAsync()
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync($"{_settings.ApiBaseUrl}client/companies");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);
                    

            return document.RootElement.Clone();
        }

        public async Task<string> CreateClientAsync(object clientPayload)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(clientPayload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_settings.ApiBaseUrl}client/companies", content);

            if (response.IsSuccessStatusCode)
            {
                return "Client created successfully in Zomentum.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return $"Error creating client: {error}";
            }
        }
    }
}

