using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.Abstractions.Results;

namespace Shared.Presentation.Convertors;

public sealed class
    Result3JsonConverter<TValue, TError1, TError2> : JsonConverter<Result<TValue, TError1, TError2>>
    where TValue : notnull
    where TError1 : Error
    where TError2 : Error
{
    private static readonly byte[] Error1TypeNameUtf8 = Encoding.UTF8.GetBytes(typeof(TError1).Name);
    private static readonly byte[] Error2TypeNameUtf8 = Encoding.UTF8.GetBytes(typeof(TError2).Name);

    public override Result<TValue, TError1, TError2> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject.");

        Result<TValue, TError1, TError2>? result = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return result ??
                       throw new JsonException("Result JSON must contain either 'value' or 'error' property.");

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueTextEquals("value"u8))
                {
                    reader.Read();
                    var value = JsonSerializer.Deserialize<TValue>(ref reader, options);
                    result = value!;
                }
                else if (reader.ValueTextEquals("error"u8))
                {
                    var checkpointReader = reader;
                    reader.Read();

                    if (reader.TokenType != JsonTokenType.StartObject)
                        throw new JsonException("Property 'error' must be an object.");

                    if (!reader.Read() || reader.TokenType != JsonTokenType.PropertyName ||
                        !reader.ValueTextEquals("$type"u8))
                    {
                        throw new JsonException("The first property in the error object must be '$type'.");
                    }

                    reader.Read();
                    if (reader.TokenType != JsonTokenType.String)
                        throw new JsonException("The value of '$type' must be a string.");

                    Type targetType;
                    if (reader.ValueTextEquals(Error1TypeNameUtf8))
                        targetType = typeof(TError1);
                    else if (reader.ValueTextEquals(Error2TypeNameUtf8))
                        targetType = typeof(TError2);
                    else
                        throw new JsonException($"Unknown error type discriminator: '{reader.GetString()}'");

                    checkpointReader.Read();

                    var errorValue = JsonSerializer.Deserialize(ref checkpointReader, targetType, options);

                    result = errorValue switch
                    {
                        TError1 e1 => (Result<TValue, TError1, TError2>)e1,
                        TError2 e2 => (Result<TValue, TError1, TError2>)e2,
                        _ => throw new JsonException("Failed to map error object to Result.")
                    };

                    reader = checkpointReader;
                }
                else
                {
                    reader.Read();
                    reader.Skip();
                }
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, Result<TValue, TError1, TError2> input,
        JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        input.Apply(
            value =>
            {
                writer.WritePropertyName("value"u8);
                JsonSerializer.Serialize(writer, value, options);
            },
            error =>
            {
                writer.WritePropertyName("error"u8);
                JsonSerializer.Serialize(writer, error, options);
            },
            error =>
            {
                writer.WritePropertyName("error"u8);
                JsonSerializer.Serialize(writer, error, options);
            }
        );
        writer.WriteEndObject();
    }
}

public sealed class Result3JsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(Result<,,>);
    }

    public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var error1Type = typeToConvert.GetGenericArguments()[1];
        var error2Type = typeToConvert.GetGenericArguments()[2];
        var converterType =
            typeof(Result3JsonConverter<,,>).MakeGenericType(valueType, error1Type, error2Type);
        return (JsonConverter?)Activator.CreateInstance(converterType);
    }
}