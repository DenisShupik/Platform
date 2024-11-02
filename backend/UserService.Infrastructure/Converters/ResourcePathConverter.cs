using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Converters;

public class ResourcePathConverter : JsonConverter<Guid>
{
    public override Guid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String) throw new JsonException("Invalid resource path format for Guid.");
        var resourcePath = reader.GetString();
        var parts = resourcePath.Split('/');
        if (parts.Length > 1 && Guid.TryParse(parts[1], out var result))
        {
            return result;
        }

        throw new JsonException("Invalid resource path format for Guid.");
    }

    public override void Write(Utf8JsonWriter writer, Guid value, JsonSerializerOptions options)
    {
        // Не реализуем, если не требуется обратная сериализация
        throw new NotImplementedException();
    }
}