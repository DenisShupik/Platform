using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Converters;

public sealed class StringifiedJsonConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected string token for representation.");
        var jsonString = reader.GetString();
        return JsonSerializer.Deserialize<T>(jsonString, options);

    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Реализуйте, если необходимо сериализовать обратно в строку
        throw new NotImplementedException();
    }
}