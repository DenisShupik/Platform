using Hangfire;
using Hangfire.PostgreSql;
using MassTransit.Logging;
using Microsoft.Extensions.Options;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Jobs;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using NotificationService.Infrastructure.Services;
using NotificationService.Presentation.Options;
using OpenTelemetry.Trace;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;
using SharedKernel.Infrastructure.Services;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services.RegisterDbContext<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services.AddHangfire((serviceProvider, configuration) => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(options =>
                {
                    var serviceOptions = serviceProvider.GetRequiredService<IOptions<NotificationServiceOptions>>()
                        .Value;
                    options.UseNpgsqlConnection(serviceOptions.ConnectionString);
                }
                ,
                new PostgreSqlStorageOptions
                {
                    SchemaName = "notifications_service_hangfire"
                }));

        builder.Services.AddHangfireServer();

        builder.Services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddScoped<IThreadSubscriptionRepository, ThreadSubscriptionRepository>()
            .AddScoped<INotificationRepository, NotificationRepository>()
            .AddScoped<INotificationDeliveryRepository, NotificationDeliveryRepository>()
            .AddScoped<NotificationJob>();

        builder.Services.AddSingleton<ServiceTokenService>();

        builder.Services.AddHttpClient<CoreServiceClient>()
            .AddHttpMessageHandler<ServiceTokenService.Handler>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(DiagnosticHeaders.DefaultListenerName)
            );
    }
}