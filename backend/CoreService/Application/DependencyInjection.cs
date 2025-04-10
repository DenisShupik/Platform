using System.Reflection;
using FluentValidation;
using Wolverine;
using Wolverine.FluentValidation;

namespace CoreService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        builder.UseWolverine(options =>
        {
            options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
            options.OptimizeArtifactWorkflow();
        });
    }
}