using System.Text.Json.Serialization;

namespace SharedKernel.Domain.Errors;

public abstract record Error
{
    [JsonPropertyName("$type")]
    [JsonPropertyOrder(-1)]
    public string Type => GetType().Name;
}