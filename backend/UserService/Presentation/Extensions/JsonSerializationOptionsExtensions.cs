using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Presentation.Convertors;
using Shared.Presentation.Extensions;

namespace UserService.Presentation.Extensions;

public static class JsonSerializationOptionsExtensions
{
    public static JsonSerializerOptions ApplyUserServiceOptions(this JsonSerializerOptions options)
    {
        options.Converters.AddRange([
            new Result2JsonConverterFactory(),
            new Result3JsonConverterFactory(),
            new Result4JsonConverterFactory(),
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        ]);
        return options;
    }
}