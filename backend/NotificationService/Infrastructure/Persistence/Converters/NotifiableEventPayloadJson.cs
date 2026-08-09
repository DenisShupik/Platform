using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Converters;

internal static class NotifiableEventPayloadJson
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        AllowOutOfOrderMetadataProperties = true
    };

    public static string Serialize(NotifiableEventPayload payload)
    {
        return JsonSerializer.Serialize(payload, SerializerOptions);
    }

    public static NotifiableEventPayload Deserialize(string json)
    {
        return JsonSerializer.Deserialize<NotifiableEventPayload>(json, SerializerOptions)
               ?? throw new JsonException($"Cannot deserialize {nameof(NotifiableEventPayload)} from JSON null.");
    }
}

internal sealed class NotifiableEventPayloadValueConverter : ValueConverter<NotifiableEventPayload, string>
{
    public NotifiableEventPayloadValueConverter()
        : base(
            payload => NotifiableEventPayloadJson.Serialize(payload),
            json => NotifiableEventPayloadJson.Deserialize(json))
    {
    }
}
