using CoreService.Presentation.Extensions;
using Shared.Presentation.Extensions;
using Shared.Presentation.Handlers;

namespace CoreService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions
                .ApplyApiContractOptions()
                .ApplyCoreServiceOptions();
        });

        builder.Services
            .RegisterApiLocalization()
            .AddServiceHealthChecks()
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

        // Keep AddOpenApi in the application project so its XML-doc source generator can intercept the call:
        // https://github.com/dotnet/aspnetcore/issues/65417
        builder.Services.AddOpenApi("openapi", options => options.SetupOpenApi());
    }
}
