using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPooledDbContextFactory<TDbContext, TDbOptions>(
        this IServiceCollection services,
        string schema
    )
        where TDbContext : DbContext
        where TDbOptions : class, IDbOptions
    {
        services.AddPooledDbContextFactory<TDbContext>(
            (provider, options) =>
            {
                var dbOptions = provider.GetRequiredService<IOptions<TDbOptions>>().Value;
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                options
                    .UseNpgsql(dbOptions.ConnectionString,
                        builder => { builder.MigrationsHistoryTable("migrations_history", schema); })
                    .UseLoggerFactory(loggerFactory)
                    .UseSnakeCaseNamingConvention();
            });

        return services;
    }
}