using Shared.Presentation.Extensions;
using Shared.Presentation.Handlers;

namespace CoreService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterOpenApi();
    }
}