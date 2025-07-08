using SharedKernel.Presentation.Extensions.ServiceCollectionExtensions;
using SharedKernel.Presentation.Handlers;
using UserService.Presentation.Filters;
using UserService.Presentation.Options;

namespace UserService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .RegisterOptions<UserServiceOptions, UserServiceOptionsValidator>(builder.Configuration)
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails()
            .RegisterSwaggerGen(options => { options.DocumentFilter<TypesDocumentFilter>(); });
    }
}