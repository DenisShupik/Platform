using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Handlers;

namespace FileService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterSwaggerGen();
    }
}