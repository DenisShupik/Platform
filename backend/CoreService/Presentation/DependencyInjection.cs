using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Presentation.Convertors;
using Shared.Presentation.Extensions;
using Shared.Presentation.Handlers;

namespace CoreService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new Result4JsonConverterFactory());
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        });

        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterOpenApi();
    }
}