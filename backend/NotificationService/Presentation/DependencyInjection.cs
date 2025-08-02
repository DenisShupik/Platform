using NotificationService.Presentation.Filters;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Handlers;

namespace NotificationService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });
    }
}