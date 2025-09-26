using Shared.Presentation.Convertors;
using Shared.Presentation.Extensions;
using Shared.Presentation.Handlers;

namespace NotificationService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new EnumSetJsonConverterFactory());
        });

        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterOpenApi();
    }
}