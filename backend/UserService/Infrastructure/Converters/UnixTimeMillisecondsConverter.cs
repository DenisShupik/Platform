using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Converters;

public sealed class UnixTimeMillisecondsConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            long milliseconds = reader.GetInt64();
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        throw new JsonException("Invalid format for Unix timestamp.");
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        // Не реализуем, если не требуется обратная сериализация
        throw new NotImplementedException();
    }
}