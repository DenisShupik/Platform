using System.Text.Json.Serialization;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserUpdatedEvent : UserEvent
{
    public sealed class RepresentationField
    {
        [JsonPropertyName("id")] public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool Enabled { get; set; }
    }

    [JsonConverter(typeof(StringifiedJsonConverter<RepresentationField>))]
    public RepresentationField Representation { get; set; }
}