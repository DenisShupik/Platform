using System.Text.Json.Serialization;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserDeletedEvent : UserEvent
{
    [JsonConverter(typeof(ResourcePathConverter))]
    [JsonPropertyName("resourcePath")]
    public Guid UserId { get; set; }
}