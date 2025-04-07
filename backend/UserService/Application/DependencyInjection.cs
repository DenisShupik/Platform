using System.Reflection;
using FluentValidation;
using SharedKernel.Options;
using Wolverine;
using Wolverine.FluentValidation;

namespace UserService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssemblyContaining<KeycloakOptions>(ServiceLifetime.Singleton)
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        builder.UseWolverine(options =>
        {
            options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
            options.OptimizeArtifactWorkflow();
        });
    }
}