using System.Text.Json;
using System.Text.Json.Serialization;

namespace UserService.Presentation.Extensions;

public static class JsonSerializationOptionsExtensions
{
    public static JsonSerializerOptions ApplyUserServiceOptions(this JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        return options;
    }
}
