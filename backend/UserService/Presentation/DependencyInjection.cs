using Shared.Presentation.Extensions;
using Shared.Presentation.Handlers;
using UserService.Presentation.Extensions;

namespace UserService.Presentation;

public static class DependencyInjection
{
    public static void AddPresentationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ApplyUserServiceOptions();
        });
        
        builder.Services
            .RegisterAuthenticationSchemes(builder.Configuration)
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails();

        // TODO: сделано так, потому что баг в Microsoft.AspNetCore.OpenApi https://github.com/dotnet/aspnetcore/issues/65417
        builder.Services.AddOpenApi("openapi", options => options.SetupOpenApi());
    }
}