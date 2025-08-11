using System.Text.Json.Serialization;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserUpdatedEvent : UserEvent
{
    public sealed class RepresentationField
    {
        [JsonPropertyName("id")] public UserId UserId { get; set; }
        public Username Username { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
    }

    [JsonConverter(typeof(StringifiedJsonConverter<RepresentationField>))]
    public RepresentationField Representation { get; set; }
}