using CoreService.Application.Interfaces;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Infrastructure.Markdown;
using CoreService.Infrastructure.Options;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using OpenTelemetry.Trace;
using ProtoBuf.Grpc.Server;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
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
            .RegisterOptions<CoreServiceOptions, CoreServiceOptionsValidator>(builder.Configuration)
            .RegisterOptions<PostContentPolicyOptions, PostContentPolicyOptionsValidator>(builder.Configuration)
            .AddSingleton<PostMarkdownProcessor>()
            .AddSingleton<IPostContentPolicy>(provider => provider.GetRequiredService<PostMarkdownProcessor>())
            .AddSingleton<IPostSearchTextProjector>(provider => provider.GetRequiredService<PostMarkdownProcessor>());

        builder.Services
            .RegisterDbContexts<ReadApplicationDbContext, WriteApplicationDbContext, T>(
                Constants.DatabaseSchema,
                valueObjectAssemblies: [typeof(ForumId).Assembly, typeof(UserId).Assembly])
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IForumReadRepository, ForumReadRepository>()
            .AddScoped<IForumWriteRepository, ForumWriteRepository>()
            .AddScoped<ICategoryReadRepository, CategoryReadRepository>()
            .AddScoped<ICategoryWriteRepository, CategoryWriteRepository>()
            .AddScoped<IThreadReadRepository, ThreadReadRepository>()
            .AddScoped<IThreadWriteRepository, ThreadWriteWriteRepository>()
            .AddScoped<IPostReadRepository, PostReadRepository>()
            .AddScoped<IPostWriteRepository, PostWriteRepository>()
            .AddScoped<IPostBookmarkReadRepository, PostBookmarkReadRepository>()
            .AddScoped<IPostBookmarkWriteRepository, PostBookmarkWriteRepository>()
            .AddScoped<ISearchReadRepository, SearchReadRepository>();

        builder.Services
            .AddDataProtection()
            .SetApplicationName(nameof(CoreService));

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
