using System.Text.Json.Serialization;

namespace SharedKernel.Tests.Dtos;

public sealed class CreateUserRequestBody
{
    public class Credential
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("value")] public string Value { get; set; }

        [JsonPropertyName("temporary")] public bool Temporary { get; set; }
    }

    [JsonPropertyName("username")] public string Username { get; set; }

    [JsonPropertyName("firstName")] public string FirstName { get; set; }

    [JsonPropertyName("lastName")] public string LastName { get; set; }

    [JsonPropertyName("email")] public string Email { get; set; }

    [JsonPropertyName("enabled")] public bool Enabled { get; set; }

    [JsonPropertyName("credentials")] public List<Credential> Credentials { get; set; }
}