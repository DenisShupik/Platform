using System.Linq.Expressions;
using System.Text.Json;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using FluentValidation;
using LinqToDB;
using LinqToDB.Mapping;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using NotificationService.Infrastructure.Options;
using NotificationService.Infrastructure.Persistence;
using NotificationService.Infrastructure.Persistence.Extensions;
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
                enableRepositoryCallDiagnostics: !builder.Environment.IsProduction(),
                jsonOptions: JsonSerializerOptions,
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

        Expression<Func<NotifiableEventPayload, ThreadId, bool>> member =
            (payload, threadId) => payload.IsPostEventFor(threadId);
        Expression<Func<NotifiableEventPayload, ThreadId, bool>> expression =
            (payload, threadId) =>
                (PostgreSqlJson.ExtractPathText(payload, "$type") == nameof(NotifiableEventPayloadType.PostAdded) ||
                 PostgreSqlJson.ExtractPathText(payload, "$type") == nameof(NotifiableEventPayloadType.PostUpdated)) &&
                Sql.ConvertTo<ThreadId>.From(PostgreSqlJson.ExtractPathText(payload, "ThreadId")) == threadId;

        LinqToDB.Linq.Expressions.MapMember(member, expression);
    }

    private static class PostgreSqlJson
    {
        [Sql.Function("jsonb_extract_path_text", ServerSideOnly = true)]
        public static string ExtractPathText(NotifiableEventPayload value, string path) =>
            throw new ServerSideOnlyException(nameof(ExtractPathText));
    }
}
