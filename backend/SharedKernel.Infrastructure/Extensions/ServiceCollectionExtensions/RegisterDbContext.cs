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

namespace SharedKernel.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static void RegisterDbContext<TDbContext, TDbOptions>(this IServiceCollection services,
        string schemaName,
        bool writeable,
        JsonSerializerOptions jsonOptions
    )
        where TDbContext : DbContext
        where TDbOptions : class, IDbOptions

    {
        services.AddDbContext<TDbContext>((provider, options) =>
        {
            var dbOptions = provider.GetRequiredService<IOptions<TDbOptions>>().Value;
            var connectionString = writeable ? dbOptions.WritableConnectionString : dbOptions.ReadonlyConnectionString;
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            dataSourceBuilder.EnableDynamicJson().ConfigureJsonOptions(jsonOptions);
            var dataSource = dataSourceBuilder.Build();

            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            options
                .UseNpgsql(dataSource,
                    builder => builder
                        .SetPostgresVersion(18, 0)
                        .MigrationsHistoryTable("migrations_history", schemaName)
                )
                .UseLinqToDB(builder => builder.AddCustomOptions(dataOptions =>
                    dataOptions.UseConnectionFactory(
                        PostgreSQLTools.GetDataProvider(PostgreSQLVersion.AutoDetect, connectionString),
                        _ => dataSource.CreateConnection()))
                )
                .UseLoggerFactory(loggerFactory)
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging()
                .UseEnumCheckConstraints();

            if (!writeable)
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }
        }, optionsLifetime: ServiceLifetime.Singleton);
    }

    public static IServiceCollection RegisterDbContexts<TReadonlyDbContext, TWritableDbContext, TDbOptions>(
        this IServiceCollection services,
        string schemaName,
        JsonSerializerOptions? jsonOptions = null
    )
        where TReadonlyDbContext : DbContext, IReadDbContext
        where TWritableDbContext : DbContext, IWriteDbContext
        where TDbOptions : class, IDbOptions
    {
        jsonOptions ??= new JsonSerializerOptions();
        jsonOptions.AllowOutOfOrderMetadataProperties = true;

        services.RegisterDbContext<TReadonlyDbContext, TDbOptions>(schemaName, false, jsonOptions);
        services.RegisterDbContext<TWritableDbContext, TDbOptions>(schemaName, true, jsonOptions);

        LinqToDBForEFTools.Initialize();

        return services;
    }
}