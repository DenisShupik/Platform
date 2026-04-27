using System.Text.Json;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Infrastructure.Interfaces;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static void RegisterDbContext<TDbContext, TDbOptions>(this IServiceCollection services,
        string schemaName,
        bool writeable,
        JsonSerializerOptions jsonOptions,
        bool useEnumCheckConstraints
    )
        where TDbContext : DbContext
        where TDbOptions : class, IDbOptions

    {
        services.AddDbContextPool<TDbContext>((provider, options) =>
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
                        .SetPostgresVersion(18, 3)
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
                .UseDiscriminatorCheckConstraints()
                ;

            if (!writeable)
            {
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            }

            if (useEnumCheckConstraints)
            {
                options.UseEnumCheckConstraints();
            }
        });
    }

    public static IServiceCollection RegisterDbContexts<TReadonlyDbContext, TWritableDbContext, TDbOptions>(
        this IServiceCollection services,
        string schemaName,
        JsonSerializerOptions? jsonOptions = null,
        bool useEnumCheckConstraints = true
    )
        where TReadonlyDbContext : DbContext, IReadDbContext
        where TWritableDbContext : DbContext, IWriteDbContext
        where TDbOptions : class, IDbOptions
    {
        jsonOptions ??= new JsonSerializerOptions();
        jsonOptions.AllowOutOfOrderMetadataProperties = true;

        services.RegisterDbContext<TReadonlyDbContext, TDbOptions>(schemaName, false, jsonOptions,
            useEnumCheckConstraints);
        services.RegisterDbContext<TWritableDbContext, TDbOptions>(schemaName, true, jsonOptions,
            useEnumCheckConstraints);

        LinqToDBForEFTools.Initialize();

        return services;
    }
}