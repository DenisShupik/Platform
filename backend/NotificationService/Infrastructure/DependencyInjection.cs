using System.Text.Json;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using FluentValidation;
using LinqToDB.Mapping;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Options;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.Customizer;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using UserService.Infrastructure.Grpc.Client;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        AllowOutOfOrderMetadataProperties = true
    };

    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<ValkeyOptions, ValkeyOptionsValidator>(builder.Configuration)
            .RegisterOptions<NotificationServiceOptions, NotificationServiceOptionsValidator>(builder.Configuration);

        builder.Services
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(
                Constants.DatabaseSchema,
                JsonSerializerOptions,
                configureLinqToDbMappings: ConfigureLinqToDbMappings,
                valueObjectAssemblies:
                [
                    typeof(NotifiableEventId).Assembly,
                    typeof(ThreadId).Assembly,
                    typeof(UserId).Assembly
                ])
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddScoped<IThreadSubscriptionWriteRepository, ThreadSubscriptionWriteRepository>()
            .AddScoped<INotifiableEventWriteRepository, NotifiableEventWriteRepository>()
            .AddScoped<INotificationReadRepository, NotificationReadRepository>()
            .AddScoped<INotificationWriteRepository, NotificationWriteRepository>();

        builder.Services.AddTickerQ(options =>
        {
            options.AddOperationalStore(efCoreOptionBuilder =>
            {
                efCoreOptionBuilder.UseApplicationDbContext<WriteApplicationDbContext>(
                    ConfigurationType.UseModelCustomizer);
                efCoreOptionBuilder.SetSchema(Constants.DatabaseSchema + "_ticker");
            });
            // options.AddDashboard(dashboardConfiguration => { dashboardConfiguration.BasePath = "/jobs"; });
        });

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddFusionCacheInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddLinqToDbInstrumentation()
            );

        builder.Services.RegisterFusionCache();

        builder.Services.RegisterGrpcRuntimeTypeModel(model =>
        {
            builder.Services.RegisterCoreServiceGrpcClient(model);
            builder.Services.RegisterUserServiceGrpcClient(model);
        });
    }

    private static void ConfigureLinqToDbMappings(MappingSchema mappingSchema)
    {
        mappingSchema.SetConverter<string, NotifiableEventPayload>(value =>
            JsonSerializer.Deserialize<NotifiableEventPayload>(value, JsonSerializerOptions));
    }
}
