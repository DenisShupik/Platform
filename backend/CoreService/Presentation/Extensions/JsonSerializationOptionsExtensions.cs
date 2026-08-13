using System.Text.Json;
using System.Text.Json.Serialization;

namespace CoreService.Presentation.Extensions;

public static class JsonSerializationOptionsExtensions
{
    public static JsonSerializerOptions ApplyCoreServiceOptions(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
