using System.Text.Json;
using System.Text.Json.Serialization;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Converters;

public class ResourcePathConverter : JsonConverter<UserId>
{
    public override UserId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Invalid resource path format for Guid.");
        var resourcePath = reader.GetString();
        var parts = resourcePath.Split('/');
        if (parts.Length > 1 && UserId.TryParse(parts[1], out var result))
        {
            return result;
        }

        throw new JsonException("Invalid resource path format for Guid.");
    }

    public override void Write(Utf8JsonWriter writer, UserId value, JsonSerializerOptions options)
    {
        // Не реализуем, если не требуется обратная сериализация
        throw new NotImplementedException();
    }
}