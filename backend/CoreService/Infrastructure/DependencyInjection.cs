using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using ProtoBuf.Meta;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;

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
            .RegisterDbContexts<ReadonlyApplicationDbContext, WritableApplicationDbContext, T>(Constants.DatabaseSchema)
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IForumReadRepository, ForumReadRepository>()
            .AddScoped<IForumRepository, ForumRepository>()
            .AddScoped<ICategoryReadRepository, CategoryReadRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IThreadReadRepository, ThreadReadRepository>()
            .AddScoped<IPostReadRepository, PostReadRepository>()
            .AddScoped<IPostRepository, PostRepository>()
            .AddScoped<IThreadRepository, ThreadRepository>();

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing.AddEntityFrameworkCoreInstrumentation());

        builder.Services.RegisterFusionCache();
        builder.RegisterCoreServiceCache(options =>
        {
            options.SetSkipMemoryCache();
            options.SetSkipDistributedCacheRead(true);
            options.SetSkipDistributedCacheWrite(false, false);
        });

        RuntimeTypeModel.Default.MapCoreServiceTypes();
        RuntimeTypeModel.Default.CompileInPlace();
        builder.Services.AddCodeFirstGrpc();
        builder.Services.AddCodeFirstGrpcReflection();
    }
}