﻿using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
        { "grant_type", "client_credentials" },
        { "client_id", _settings.ClientId },
        { "client_secret", _settings.ClientSecret },
        { "scope", "all" } // You may test with "openid" or leave out if it causes issues
    };

            var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Token Response:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get token: {response.StatusCode} - {content}");
            }

            using var doc = JsonDocument.Parse(content);
            var token = doc.RootElement.GetProperty("access_token").GetString();

            Console.WriteLine("Access Token: " + token);
            return token;
        }

        //public async Task<string> GetAccessTokenAsync()
        //{
        //    var client = _httpClientFactory.CreateClient();

        //    var form = new Dictionary<string, string>
        //{
        //    {"grant_type", "client_credentials"},
        //    {"client_id", _settings.ClientId},
        //    {"client_secret", _settings.ClientSecret},
        //    {"scope","all" }
        //};

        //    var response = await client.PostAsync(_settings.TokenUrl, new FormUrlEncodedContent(form));
        //    response.EnsureSuccessStatusCode();

        //    var content = await response.Content.ReadAsStringAsync();
        //    using var doc = JsonDocument.Parse(content);
        //    return doc.RootElement.GetProperty("access_token").GetString();

        //}       

        //public async Task<string> GetClientsAsync()
        //{
        //    var token = await GetAccessTokenAsync();

        //    var client = _httpClientFactory.CreateClient();
        //    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        //    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //    var requestUrl = $"{_settings.ApiBaseUrl}/client";


        //    var response = await client.GetAsync(requestUrl);

        //    var responseBody = await response.Content.ReadAsStringAsync();

        //    Console.WriteLine($"Response Status: {response.StatusCode}");
        //    Console.WriteLine($"Response Body: {responseBody}");

        //    if (!response.IsSuccessStatusCode)
        //    {
        //        throw new Exception($"API Error: {response.StatusCode}\n{responseBody}");
        //    }

        //    return responseBody;

        //}

        public async Task<HttpResponseMessage> CreateClientAsync(object payload)
        {

            var token = await GetAccessTokenAsync();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            

            var content = new StringContent(JsonSerializer.Serialize(payload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await client.PostAsync($"{_settings.ApiBaseUrl}Client", content);
        }


    }
}
