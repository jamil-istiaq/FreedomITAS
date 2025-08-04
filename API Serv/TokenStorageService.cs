
using System.Text.Json;

namespace FreedomITAS.API_Serv
{
    public class TokenStorageService
    {
        private const string TokenFilePath = "Data/TokenStore.json";

        public async Task<(string AccessToken, string RefreshToken, DateTime ExpiresAt)> LoadTokensAsync()
        {
            if (!File.Exists(TokenFilePath))
                throw new FileNotFoundException("Token storage file not found.");

            var json = await File.ReadAllTextAsync(TokenFilePath);
            var doc = JsonDocument.Parse(json);
            var accessToken = doc.RootElement.GetProperty("access_token").GetString();
            var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
            var expiresAt = doc.RootElement.GetProperty("expires_at").GetDateTime();

            return (accessToken!, refreshToken!, expiresAt);
        }

        public async Task SaveTokensAsync(string accessToken, string refreshToken, int expiresInSeconds)
        {
            var data = new
            {
                access_token = accessToken,
                refresh_token = refreshToken,
                expires_at = DateTime.UtcNow.AddSeconds(expiresInSeconds - 60) // buffer
            };

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(TokenFilePath)!);
            await File.WriteAllTextAsync(TokenFilePath, json);
        }
    }
}
