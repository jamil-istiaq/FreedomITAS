using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using FreedomITAS.API_Settings;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text;
using Azure;
using System.Net;

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

        //public async Task<JsonElement> GetClientsAsync()
        //{
        //    var client = _httpClientFactory.CreateClient();

        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    var response = await client.GetAsync($"{_settings.ApiBaseUrl}client/companies");
        //    response.EnsureSuccessStatusCode();

        //    var json = await response.Content.ReadAsStringAsync();

        //    using var document = JsonDocument.Parse(json);


        //    return document.RootElement.Clone();
        //}

        public async Task<string> RefreshAccessTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                grant_type = "refresh_token",
                refresh_token = _settings.RefreshToken
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.zomentum.com/v1/oauth/access-token", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jsonDoc = JsonDocument.Parse(responseContent);
                return jsonDoc.RootElement.GetProperty("access_token").GetString();
            }
            else
            {
                throw new Exception($"Failed to refresh token: {responseContent}");
            }
        }

        private async Task<HttpResponseMessage> PostToZomentumAsync(object payload, string token)
        {
            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await client.PostAsync($"{_settings.ApiBaseUrl.TrimEnd('/')}/client/companies", content);
        }
        public async Task<string> CreateClientAsync(object clientPayload)
        {
            //var client = _httpClientFactory.CreateClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.AccessToken);

            var response = await PostToZomentumAsync(clientPayload, _settings.AccessToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var newAccessToken = await RefreshAccessTokenAsync();
                _settings.AccessToken = newAccessToken; 
                response = await PostToZomentumAsync(clientPayload, newAccessToken);
            }

            var content = await response.Content.ReadAsStringAsync();
            return response.IsSuccessStatusCode
                ? "Client created successfully in Zomentum."
                : $"Error creating client: {content}";

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //var json = JsonSerializer.Serialize(clientPayload);
            //var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var response = await client.PostAsync($"{_settings.ApiBaseUrl}client/companies", content);

            //if (response.IsSuccessStatusCode)
            //{
            //    return "Client created successfully in Zomentum.";
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync();
            //    return $"Error creating client: {error}";
            //}
        }
    }
}

