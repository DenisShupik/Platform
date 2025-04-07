using System.Text.Json.Serialization;
using SharedKernel.Domain.ValueObjects;
using UserService.Infrastructure.Converters;

namespace UserService.Infrastructure.Events;

public sealed class UserRegisteredEvent : UserEvent
{
    public sealed class DetailsField
    {
        public string Email { get; set; }
        public string Username { get; set; }
    }

    [JsonConverter(typeof(UnixTimeMillisecondsConverter))]
    [JsonPropertyName("time")]
    public DateTime RegisteredAt { get; set; }

    public UserId UserId { get; set; }
    public DetailsField Details { get; set; }
}