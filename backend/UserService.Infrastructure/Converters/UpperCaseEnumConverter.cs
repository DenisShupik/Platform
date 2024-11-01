using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserService.Infrastructure.Converters;

public sealed class UpperCaseEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (Enum.TryParse(typeof(T), value, true, out var result))
        {
            return (T)result;
        }
        throw new JsonException($"Unknown value for enum {typeof(T).Name}: {value}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var enumValue = value.ToString().ToUpperInvariant();
        writer.WriteStringValue(enumValue);
    }
}