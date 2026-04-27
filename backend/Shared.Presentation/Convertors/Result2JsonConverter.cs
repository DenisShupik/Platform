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
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Ожидался StartObject.");
        }

        TValue? parsedValue = default;
        TError1? parsedError = null;
        
        var hasValue = false;
        var hasError = false;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Ожидалось название свойства.");
            }
            
            if (reader.ValueTextEquals("value"u8))
            {
                if (hasError) throw new JsonException("Невалидный Result: найдено и 'value', и 'error'.");
                if (hasValue) throw new JsonException("Дублирующееся свойство 'value'.");

                reader.Read();
                parsedValue = JsonSerializer.Deserialize<TValue>(ref reader, options);
                hasValue = true;
            }
            else if (reader.ValueTextEquals("error"u8))
            {
                if (hasValue) throw new JsonException("Невалидный Result: найдено и 'value', и 'error'.");
                if (hasError) throw new JsonException("Дублирующееся свойство 'error'.");

                reader.Read();
                parsedError = JsonSerializer.Deserialize<TError1>(ref reader, options);
                hasError = true;
            }
            else
            {
                reader.Skip();
            }
        }
        
        return (hasValue, hasError) switch
        {
            (true, false) => parsedValue!,
            (false, true) => parsedError!,
            _ => throw new JsonException("Result должен содержать либо 'value', либо 'error'.")
        };
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