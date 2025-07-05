using System.Reflection;
using FluentValidation;
using JasperFx.CodeGeneration;
using NotificationService.Application.UseCases;
using Wolverine;
using Wolverine.FluentValidation;

namespace NotificationService.Application;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

        builder.UseWolverine(options =>
        {
            options.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
            options.CodeGeneration.TypeLoadMode = TypeLoadMode.Auto;
        });
    }
}