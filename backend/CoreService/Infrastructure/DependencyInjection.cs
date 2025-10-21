using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using Shared.Application.Interfaces;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Interfaces;

namespace CoreService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services
            .AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, ServiceLifetime.Singleton)
            .RegisterOptions<CoreServiceOptions, CoreServiceOptionsValidator>(builder.Configuration);

        builder.Services
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IAccessReadRepository, AccessReadRepository>()
            .AddScoped<IAccessWriteRepository, AccessWriteRepository>()
            .AddScoped<IPortalReadRepository, PortalReadRepository>()
            .AddScoped<IPortalWriteRepository, PortalWriteRepository>()
            .AddScoped<IForumReadRepository, ForumReadRepository>()
            .AddScoped<IForumWriteRepository, ForumWriteRepository>()
            .AddScoped<ICategoryReadRepository, CategoryReadRepository>()
            .AddScoped<ICategoryWriteRepository, CategoryWriteRepository>()
            .AddScoped<IThreadReadRepository, ThreadReadRepository>()
            .AddScoped<IPostReadRepository, PostReadRepository>()
            .AddScoped<IPostWriteRepository, PostWriteRepository>()
            .AddScoped<IThreadWriteRepository, ThreadWriteWriteRepository>()
            .AddScoped<IPolicyReadRepository, PolicyReadRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing
                .AddEntityFrameworkCoreInstrumentation()
                .AddLinqToDbInstrumentation()
            );

        builder.Services.RegisterFusionCache();
        builder.Services.RegisterCoreServiceCache(options =>
        {
            options.SetSkipMemoryCache();
            options.SetSkipDistributedCacheRead(true);
            options.SetSkipDistributedCacheWrite(false, false);
        });

        builder.Services.RegisterGrpcRuntimeTypeModel(model =>
        {
            model.MapCoreServiceTypes();
            model.CompileInPlace();
        });
        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }
}