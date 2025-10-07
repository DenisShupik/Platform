using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.Abstractions.Results;

namespace Shared.Presentation.Convertors;

public sealed class
    Result4JsonConverter<TValue, TError1, TError2, TError3> : JsonConverter<Result<TValue, TError1, TError2, TError3>>
    where TValue : notnull
    where TError1 : Error
    where TError2 : Error
    where TError3 : Error
{
    public override Result<TValue, TError1, TError2, TError3> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, Result<TValue, TError1, TError2, TError3> input,
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
            },
            error =>
            {
                writer.WritePropertyName("error");
                JsonSerializer.Serialize(writer, error, options);
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

public sealed class Result4JsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(Result<,,,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var error1Type = typeToConvert.GetGenericArguments()[1];
        var error2Type = typeToConvert.GetGenericArguments()[2];
        var error3Type = typeToConvert.GetGenericArguments()[3];
        var converterType =
            typeof(Result4JsonConverter<,,,>).MakeGenericType(valueType, error1Type, error2Type, error3Type);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}