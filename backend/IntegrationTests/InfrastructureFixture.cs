using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using Shared.Tests.Services;
using Xunit;

[assembly: AssemblyFixture(typeof(IntegrationTests.InfrastructureFixture))]

namespace IntegrationTests;

public sealed class InfrastructureFixture : IAsyncLifetime
{
    public readonly DistributedApplication Infrastructure;
    public readonly UserTokenService UserTokenService;
    public readonly ServiceTokenService ServiceTokenService;

    private string? _connectionString;

    public readonly KeycloakOptions KeycloakOptions;
    public readonly RabbitMqOptions RabbitMqOptions;

    public InfrastructureFixture()
    {
        KeycloakOptions = new KeycloakOptions
        {
            MetadataAddress = "http://localhost:8080/realms/app-test/.well-known/openid-configuration",
            Issuer = "http://localhost:8080/realms/app-test",
            Audience = "app-test-user",
            Realm = "app-test",
            ServiceClientId = "app-service",
            ServiceClientSecret = "4MZ1td4U3CSSqjwrOkgLRukvEcEe9eeN"
        };

        RabbitMqOptions = new RabbitMqOptions
        {
            Host = "localhost",
            Username = "admin",
            Password = "12345678"
        };

        Infrastructure = DistributedApplicationTestingBuilder.CreateAsync<Projects.DevEnv>([
                "DcpPublisher:RandomizePorts=false",
                "Seeding=false",
                "DisableServices=true",
                $"KeycloakOptions:MetadataAddress={KeycloakOptions.MetadataAddress}",
                $"KeycloakOptions:Issuer={KeycloakOptions.Issuer}",
                $"KeycloakOptions:Audience={KeycloakOptions.Audience}",
                $"KeycloakOptions:Realm={KeycloakOptions.Realm}",
                $"RabbitMqOptions:Host={RabbitMqOptions.Host}",
                $"RabbitMqOptions:Username={RabbitMqOptions.Username}",
                $"RabbitMqOptions:Password={RabbitMqOptions.Password}"
            ])
            .GetAwaiter()
            .GetResult()
            .Build();

        UserTokenService = new UserTokenService(new OptionsWrapper<KeycloakOptions>(KeycloakOptions));
        ServiceTokenService = new ServiceTokenService(new OptionsWrapper<KeycloakOptions>(KeycloakOptions));
    }

    public async ValueTask InitializeAsync()
    {
        await Infrastructure.StartAsync();
        _connectionString = await Infrastructure.GetConnectionStringAsync("postgres");
    }

    public async ValueTask DisposeAsync()
    {
        await Infrastructure.DisposeAsync();
        UserTokenService.Dispose();
        ServiceTokenService.Dispose();
    }

    public DbContextConnectionStrings CreateDatabase(string database)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
        createCmd.ExecuteNonQuery();
        var readonlyBuilder = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = database,
            Username = "readonly_user"
        };
        var writeableBuilder = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = database
        };
        return new DbContextConnectionStrings
        {
            ReadonlyDbContext = readonlyBuilder,
            WritableDbContext = writeableBuilder
        };
    }
}