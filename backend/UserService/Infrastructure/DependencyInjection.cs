using FluentValidation;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Options;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Grpc.Contracts;
using UserService.Infrastructure.Options;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;
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
            .AddScoped(typeof(GetUsersPagedQueryHandler<>));
        
        builder.Services
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUserReadRepository, UserReadRepository>()
            .AddScoped<IUserWriteRepository, UserWriteRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
            );

        builder.Services.RegisterFusionCache();
        builder.Services.RegisterUserServiceCache(options =>
        {
            options.SetSkipMemoryCache();
            options.SetSkipDistributedCacheRead(true);
            options.SetSkipDistributedCacheWrite(false, false);
        });

        builder.Services.RegisterGrpcRuntimeTypeModel(model =>
        {
            model.MapUserServiceTypes();
            model.CompileInPlace();
        });
        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }
}