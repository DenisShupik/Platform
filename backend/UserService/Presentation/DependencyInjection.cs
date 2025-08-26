using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.Handlers;
using UserService.Presentation.Filters;

namespace UserService.Presentation;

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