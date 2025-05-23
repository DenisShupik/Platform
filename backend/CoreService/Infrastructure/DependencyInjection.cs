using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Persistence;
using CoreService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using SharedKernel.Application.Interfaces;
using SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;
using SharedKernel.Infrastructure.Interfaces;

namespace CoreService.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices<T>(this IHostApplicationBuilder builder)
        where T : class, IDbOptions
    {
        builder.Services.RegisterPooledDbContextFactory<ApplicationDbContext, T>(Constants.DatabaseSchema);

        builder.Services.AddScoped<ApplicationDbContext>(serviceProvider => serviceProvider
            .GetRequiredService<IDbContextFactory<ApplicationDbContext>>()
            .CreateDbContext()
        );

        builder.Services
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IForumReadRepository, ForumReadRepository>()
            .AddScoped<IForumRepository, ForumRepository>()
            .AddScoped<ICategoryReadRepository, CategoryReadRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IThreadReadRepository, ThreadReadRepository>()
            .AddScoped<IPostReadRepository, PostReadRepository>()
            .AddScoped<IPostRepository, PostRepository>()
            .AddScoped<IThreadRepository, ThreadRepository>()
            ;

        builder.Services
            .RegisterOpenTelemetry(builder.Environment.ApplicationName)
            .WithTracing(tracing => tracing.AddEntityFrameworkCoreInstrumentation())
            ;
    }
}