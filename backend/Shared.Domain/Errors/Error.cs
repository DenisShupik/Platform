using System.Text.Json.Serialization;

namespace Shared.Domain.Errors;

[JsonPolymorphic]
public abstract record Error
{
    [JsonPropertyOrder(-1)]
    [JsonPropertyName("$type")]
    public string Type => GetType().Name;
}