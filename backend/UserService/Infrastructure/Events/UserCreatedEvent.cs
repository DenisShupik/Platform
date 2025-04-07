using System.Text.Json.Serialization;
using SharedKernel.Domain.ValueObjects;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserCreatedEvent : UserEvent
{
    public sealed class RepresentationField
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
    }

    [JsonConverter(typeof(UnixTimeMillisecondsConverter))]
    [JsonPropertyName("time")]
    public DateTime CreatedAt { get; set; }

    [JsonConverter(typeof(ResourcePathConverter))]
    [JsonPropertyName("resourcePath")]
    public UserId UserId { get; set; }

    [JsonConverter(typeof(StringifiedJsonConverter<RepresentationField>))]
    public RepresentationField Representation { get; set; }
}