using System.Text.Json.Serialization;

namespace SharedKernel.Tests.Dtos;

public sealed class TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; }

    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
}