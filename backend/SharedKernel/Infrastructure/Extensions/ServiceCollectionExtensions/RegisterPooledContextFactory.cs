using System.Text.Json;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedKernel.Infrastructure.Interfaces;

namespace SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterPooledDbContextFactory<TDbContext, TDbOptions>(
        this IServiceCollection services,
        string schemaName,
        JsonSerializerOptions? jsonOptions = null
    )
        where TDbContext : DbContext
        where TDbOptions : class, IDbOptions
    {
        jsonOptions ??= new JsonSerializerOptions();
        jsonOptions.AllowOutOfOrderMetadataProperties = true;

        services.AddPooledDbContextFactory<TDbContext>((provider, options) =>
        {
            var dbOptions = provider.GetRequiredService<IOptions<TDbOptions>>().Value;

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(dbOptions.ConnectionString);

            dataSourceBuilder.EnableDynamicJson().ConfigureJsonOptions(jsonOptions);
            var dataSource = dataSourceBuilder.Build();

            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            options
                .UseNpgsql(dataSource,
                    builder =>
                    {
                        builder
                            .SetPostgresVersion(17, 4)
                            .MigrationsHistoryTable("migrations_history", schemaName)
                            ;
                    })
                .UseLinqToDB(builder =>
                {
                    builder.AddCustomOptions(o =>
                        o.UseConnectionFactory(
                            PostgreSQLTools.GetDataProvider(PostgreSQLVersion.AutoDetect, dbOptions.ConnectionString),
                            _ => dataSource.CreateConnection()));
                })
                .UseLoggerFactory(loggerFactory)
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging()
                ;
        });

        LinqToDBForEFTools.Initialize();

        return services;
    }
}