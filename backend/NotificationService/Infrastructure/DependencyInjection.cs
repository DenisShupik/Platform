using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using CoreService.Infrastructure.Grpc.Contracts;
using FluentValidation;
using LinqToDB.Mapping;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Clients;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Converters;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.ClientFactory;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using UserService.Infrastructure.Grpc.Contracts;

namespace NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<ValkeyOptions, ValkeyOptionsValidator>(builder.Configuration)
            .RegisterOptions<ServiceAccountOptions, ServiceAccountOptionsValidator>(builder.Configuration)
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
            model.MapCoreServiceTypes();
            model.MapUserServiceTypes();
        });
        builder.Services.AddSingleton<ServiceTokenService>();
        builder.Services.AddTransient<ServiceTokenService.Handler>();
        builder.Services.AddCoreServiceGrpcClient(new Uri("http://localhost:8011"))
            .AddHttpMessageHandler<ServiceTokenService.Handler>();
        builder.Services.AddCodeFirstGrpcClient<IGrpcUserService>(options =>
            {
                // TODO: replace with service discovery/options.
                options.Address = new Uri("http://localhost:8021");
            })
            .AddHttpMessageHandler<ServiceTokenService.Handler>();
        builder.Services.AddSingleton<IThreadAccessReader, CoreThreadAccessReader>();
        builder.Services.AddSingleton<IUserDirectoryReader, UserDirectoryReader>();
    }

    private static void ConfigureLinqToDbMappings(MappingSchema mappingSchema)
    {
        mappingSchema.SetConverter<string, NotifiableEventPayload>(NotifiableEventPayloadJson.Deserialize);
        mappingSchema.SetConverter<NotifiableEventPayload, string>(NotifiableEventPayloadJson.Serialize);
    }
}
