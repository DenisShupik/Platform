using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Presentation.Convertors;
using Shared.Presentation.Generator.Attributes;

namespace Shared.Presentation.Extensions;

[GenerateResultJsonConverters]
public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ApplyApiContractOptions(this JsonSerializerOptions options)
    {
        options.NumberHandling = JsonNumberHandling.Strict;
        if (!options.Converters.Contains(ResultJsonConverterFactory.Instance))
            options.Converters.Add(ResultJsonConverterFactory.Instance);
        return options;
    }
}
