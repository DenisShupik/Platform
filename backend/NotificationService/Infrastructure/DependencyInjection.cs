using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using FluentValidation;
using LinqToDB.Mapping;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Converters;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Options;
using UserService.Infrastructure.Grpc.Client;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
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
                enableRepositoryCallDiagnostics: !builder.Environment.IsProduction(),
                configureLinqToDbMappings: ConfigureLinqToDbMappings,
                valueObjectAssemblies:
                [
                    typeof(NotifiableEventId).Assembly,
                    typeof(ThreadId).Assembly,
                    typeof(UserId).Assembly
                ])
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddRepository<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddRepository<IThreadSubscriptionWriteRepository, ThreadSubscriptionWriteRepository>()
            .AddRepository<INotifiableEventWriteRepository, NotifiableEventWriteRepository>()
            .AddRepository<INotificationReadRepository, NotificationReadRepository>()
            .AddRepository<INotificationWriteRepository, NotificationWriteRepository>();

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
        mappingSchema.SetConverter<string, NotifiableEventPayload>(NotifiableEventPayloadJson.Deserialize);
        mappingSchema.SetConverter<NotifiableEventPayload, string>(NotifiableEventPayloadJson.Serialize);
    }
}
