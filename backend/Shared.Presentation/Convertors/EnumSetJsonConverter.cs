using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Domain.Abstractions;

namespace Shared.Presentation.Convertors;

public sealed class EnumSetJsonConverter<T> : JsonConverter<EnumSet<T>>
    where T : struct, Enum
{
    public override EnumSet<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var items = JsonSerializer.Deserialize<HashSet<T>>(ref reader, options);

        if (items == null) throw new JsonException("Expected JSON array for EnumSet.");

        return EnumSet<T>.TryCreate(items, out var result, out var error) ? result : throw new JsonException(error);
    }

    public override void Write(Utf8JsonWriter writer, EnumSet<T> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize<HashSet<T>>(writer, value, options);
    }
}

public sealed class EnumSetJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(EnumSet<>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var itemType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(EnumSetJsonConverter<>).MakeGenericType(itemType);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}