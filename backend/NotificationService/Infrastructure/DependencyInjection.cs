using MassTransit.Logging;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;
using Wolverine.Persistence;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    { 
        builder.Services.RegisterPooledDbContextFactory<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services.AddScoped<ApplicationDbContext>(serviceProvider => serviceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
            .CreateDbContext()
        );

        builder.Services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddScoped<IThreadSubscriptionRepository, ThreadSubscriptionRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(DiagnosticHeaders.DefaultListenerName)
            );
    }
}