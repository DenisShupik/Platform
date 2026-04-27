using System.Text.Json.Serialization;

namespace Shared.Tests.Dtos;

public sealed class AssignRoleToUserRequestBody : List<AssignRoleToUserRequestBody.Client>
{
    public sealed class Client
    {
        [JsonPropertyName("id")] public required Guid ClientId { get; init; }
        [JsonPropertyName("name")] public required string Role { get; init; }
    }
}