using System.Reflection;
using System.Text.Json;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Infrastructure.Diagnostics;
using Shared.Infrastructure.Interfaces;
using Shared.Infrastructure.Persistence;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static void RegisterDbContext<TDbContext, TDbOptions>(this IServiceCollection services,
        string schemaName,
        bool writeable,
        bool enableRepositoryCallDiagnostics,
        JsonSerializerOptions jsonOptions,
        bool useEnumCheckConstraints,
        MappingSchema mappingSchema
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
            var efRepositoryCallInterceptor = enableRepositoryCallDiagnostics
                ? provider.GetRequiredService<EfRepositoryCallCommandInterceptor>()
                : null;
            var linqToDbRepositoryCallInterceptor = enableRepositoryCallDiagnostics
                ? provider.GetRequiredService<LinqToDbRepositoryCallCommandInterceptor>()
                : null;
            options
                .UseNpgsql(dataSource,
                    builder => builder
                        .SetPostgresVersion(18, 3)
                        .MigrationsHistoryTable("migrations_history", schemaName)
                )
                .UseLinqToDB(builder =>
                {
                    builder.AddMappingSchema(mappingSchema);
                    builder.AddCustomOptions(dataOptions =>
                    {
                        var configuredOptions = dataOptions.UseConnectionFactory(
                            PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v18, connectionString),
                            _ => dataSource.CreateConnection());

                        return linqToDbRepositoryCallInterceptor is null
                            ? configuredOptions
                            : configuredOptions.UseInterceptor(linqToDbRepositoryCallInterceptor);
                    });
                })
                .UseLoggerFactory(loggerFactory)
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging()
                .UseDiscriminatorCheckConstraints();

            if (efRepositoryCallInterceptor is not null)
            {
                options.AddInterceptors(efRepositoryCallInterceptor);
            }

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
        bool enableRepositoryCallDiagnostics,
        JsonSerializerOptions? jsonOptions = null,
        bool useEnumCheckConstraints = true,
        Action<MappingSchema>? configureLinqToDbMappings = null,
        params Assembly[] valueObjectAssemblies
    )
        where TReadonlyDbContext : DbContext, IReadDbContext
        where TWritableDbContext : DbContext, IWriteDbContext
        where TDbOptions : class, IDbOptions
    {
        jsonOptions ??= new JsonSerializerOptions();
        jsonOptions.AllowOutOfOrderMetadataProperties = true;

        var mappingSchema = ValueObjectConversions.CreateLinqToDbMappingSchema(valueObjectAssemblies);
        configureLinqToDbMappings?.Invoke(mappingSchema);

        if (enableRepositoryCallDiagnostics)
        {
            services.TryAddSingleton<RepositoryCallContextAccessor>();
            services.TryAddSingleton<EfRepositoryCallCommandInterceptor>();
            services.TryAddSingleton<LinqToDbRepositoryCallCommandInterceptor>();
        }

        services.RegisterDbContext<TReadonlyDbContext, TDbOptions>(schemaName, false,
            enableRepositoryCallDiagnostics, jsonOptions,
            useEnumCheckConstraints, mappingSchema);
        services.RegisterDbContext<TWritableDbContext, TDbOptions>(schemaName, true,
            enableRepositoryCallDiagnostics, jsonOptions,
            useEnumCheckConstraints, mappingSchema);

        LinqToDBForEFTools.Initialize();

        return services;
    }
}
