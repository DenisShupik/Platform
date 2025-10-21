using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.Abstractions.Results;

namespace Shared.Presentation.Convertors;

public sealed class
    Result2JsonConverter<TValue, TError1> : JsonConverter<Result<TValue, TError1>>
    where TValue : notnull
    where TError1 : Error
{
    public override Result<TValue, TError1> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Result<TValue, TError1> input,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        input.Apply(
            value =>
            {
                writer.WritePropertyName("value");
                JsonSerializer.Serialize(writer, value, options);
            },
            error =>
            {
                writer.WritePropertyName("error");
                JsonSerializer.Serialize(writer, error, options);
            }
        );
        writer.WriteEndObject();
    }
}

public sealed class Result2JsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(Result<,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var error1Type = typeToConvert.GetGenericArguments()[1];
        var converterType =
            typeof(Result2JsonConverter<,>).MakeGenericType(valueType, error1Type);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}