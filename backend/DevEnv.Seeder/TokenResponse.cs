using System.Text.Json.Serialization;

namespace DevEnv.Seeder;

public sealed class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}