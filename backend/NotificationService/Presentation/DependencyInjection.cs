using NotificationService.Presentation.Filters;
using NotificationService.Presentation.Options;
using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Handlers;
using SharedKernel.Presentation.Options;

namespace NotificationService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterOptions<ValkeyOptions, ValkeyOptionsValidator>(builder.Configuration)
            .RegisterOptions<NotificationServiceOptions, NotificationServiceOptionsValidator>(builder.Configuration)
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });
    }
}