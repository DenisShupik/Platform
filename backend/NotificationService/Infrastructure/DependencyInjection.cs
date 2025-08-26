using System.Text.Json;
using CoreService.Infrastructure.Grpc.Client;
using FluentValidation;
using LinqToDB.Mapping;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Repositories;
using OpenTelemetry.Trace;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions;
using SharedKernel.Infrastructure.Interfaces;
using SharedKernel.Infrastructure.Options;
using TickerQ.DependencyInjection;
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
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IThreadSubscriptionReadRepository, ThreadSubscriptionReadRepository>()
            .AddScoped<IThreadSubscriptionWriteRepository, ThreadSubscriptionWriteRepository>()
            .AddScoped<INotifiableEventWriteRepository, NotifiableEventWriteRepository>()
            .AddScoped<INotificationReadRepository, NotificationReadRepository>()
            .AddScoped<INotificationWriteRepository, NotificationWriteRepository>();

        builder.Services.AddTickerQ(options =>
        {
            options.AddOperationalStore<WriteApplicationDbContext>(efCoreOptionBuilder =>
            {
                efCoreOptionBuilder.CancelMissedTickersOnAppStart();
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

        MappingSchema.Default.SetConverter<string, NotifiableEventPayload>(value =>
            JsonSerializer.Deserialize<NotifiableEventPayload>(value, JsonSerializerOptions));
    }
}