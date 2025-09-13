using System.Text.Json.Serialization;

namespace Shared.Domain.Errors;

public abstract record Error
{
    [JsonPropertyName("$type")]
    [JsonPropertyOrder(-1)]
    public string Type => GetType().Name;
}