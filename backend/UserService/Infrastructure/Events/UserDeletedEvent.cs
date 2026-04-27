using System.Text.Json.Serialization;
using Shared.Domain.ValueObjects;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserDeletedEvent : UserEvent
{
    [JsonConverter(typeof(ResourcePathConverter))]
    [JsonPropertyName("resourcePath")]
    public UserId UserId { get; set; }
}