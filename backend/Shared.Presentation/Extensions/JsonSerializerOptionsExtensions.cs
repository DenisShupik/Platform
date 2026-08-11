using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Presentation.Extensions;

public static class JsonSerializerOptionsExtensions
{
    public static JsonSerializerOptions ApplyApiContractOptions(this JsonSerializerOptions options)
    {
        options.NumberHandling = JsonNumberHandling.Strict;
        return options;
    }
}
