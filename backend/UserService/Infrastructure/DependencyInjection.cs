using FluentValidation;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Grpc;
using SharedKernel.Infrastructure.Interfaces;
using SharedKernel.Infrastructure.Options;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Grpc.Contracts;
using UserService.Infrastructure.Options;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;
using ZiggyCreatures.Caching.Fusion;
using Constants = UserService.Infrastructure.Persistence.Constants;

namespace UserService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<UserServiceOptions, UserServiceOptionsValidator>(builder.Configuration)
            .RegisterOptions<RabbitMqOptions, RabbitMqOptionsValidator>(builder.Configuration)
            .RegisterOptions<ValkeyOptions, ValkeyOptionsValidator>(builder.Configuration);

        builder.Services
            .RegisterDbContexts<ReadonlyApplicationDbContext, WritableApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUserReadRepository, UserReadRepository>()
            .AddScoped<IUserWriteRepository, UserWriteRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
            );

        builder.Services.RegisterFusionCache();
        builder.RegisterUserServiceCache(options =>
        {
            options.SetSkipMemoryCache();
            options.SetSkipDistributedCacheRead(true);
            options.SetSkipDistributedCacheWrite(false, false);
        });

        RuntimeTypeModel.Default.MapUserServiceTypes();
        RuntimeTypeModel.Default.CompileInPlace();
        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }
}