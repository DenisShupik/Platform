using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.Options;
using Npgsql;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using Shared.Tests.Services;
using TUnit.Core.Interfaces;

namespace IntegrationTests;

public sealed class InfrastructureFixture : IAsyncInitializer, IAsyncDisposable
{
    private DistributedApplication Infrastructure { get; set; } = null!;
    public readonly UserTokenService UserTokenService;
    public readonly ServiceTokenService ServiceTokenService;
    public readonly KeycloakAdminTokenService KeycloakAdminTokenService;

    private string? _connectionString;

    public readonly KeycloakOptions KeycloakOptions;
    public readonly InternalApiOptions InternalApiOptions;
    public readonly ServiceAccountOptions NotificationServiceAccountOptions;
    public readonly KeycloakAdminOptions KeycloakAdminOptions;
    public readonly RabbitMqOptions RabbitMqOptions;

    public InfrastructureFixture()
    {
        KeycloakOptions = new KeycloakOptions
        {
            MetadataAddress = "http://localhost:8080/realms/app-test/.well-known/openid-configuration",
            Issuer = "http://localhost:8080/realms/app-test",
            Audience = "app-test-user",
            InternalAudience = "app-test-internal",
            Realm = "app-test"
        };
        InternalApiOptions = new InternalApiOptions
        {
            CoreServiceClientId = "core-service",
            NotificationServiceClientId = "notification-service",
            ProvisioningServiceClientId = "dev-provisioner"
        };
        NotificationServiceAccountOptions = new ServiceAccountOptions
        {
            ClientId = InternalApiOptions.NotificationServiceClientId,
            ClientSecret = "notification-service-development-secret"
        };
        KeycloakAdminOptions = new KeycloakAdminOptions
        {
            ClientId = "app-identity-admin",
            ClientSecret = "G7zZhzWNgHvhHfFTYRJzfRBR1hQnDaM7"
        };

        RabbitMqOptions = new RabbitMqOptions
        {
            Host = "localhost",
            Username = "admin",
            Password = "12345678"
        };

        UserTokenService = new UserTokenService(new OptionsWrapper<KeycloakOptions>(KeycloakOptions));
        ServiceTokenService = new ServiceTokenService(
            new OptionsWrapper<KeycloakOptions>(KeycloakOptions),
            new OptionsWrapper<ServiceAccountOptions>(NotificationServiceAccountOptions));
        KeycloakAdminTokenService = new KeycloakAdminTokenService(
            new OptionsWrapper<KeycloakOptions>(KeycloakOptions),
            new OptionsWrapper<KeycloakAdminOptions>(KeycloakAdminOptions));
    }

    public async Task InitializeAsync()
    {
        Infrastructure = (await DistributedApplicationTestingBuilder.CreateAsync<Projects.DevEnv>([
                "DcpPublisher:RandomizePorts=false",
                "Seeding=false",
                "DisableServices=true",
                $"KeycloakOptions:MetadataAddress={KeycloakOptions.MetadataAddress}",
                $"KeycloakOptions:Issuer={KeycloakOptions.Issuer}",
                $"KeycloakOptions:Audience={KeycloakOptions.Audience}",
                $"KeycloakOptions:InternalAudience={KeycloakOptions.InternalAudience}",
                $"KeycloakOptions:Realm={KeycloakOptions.Realm}",
                $"KeycloakAdminOptions:ClientId={KeycloakAdminOptions.ClientId}",
                $"KeycloakAdminOptions:ClientSecret={KeycloakAdminOptions.ClientSecret}",
                $"RabbitMqOptions:Host={RabbitMqOptions.Host}",
                $"RabbitMqOptions:Username={RabbitMqOptions.Username}",
                $"RabbitMqOptions:Password={RabbitMqOptions.Password}"
            ]))
            .Build();

        await Infrastructure.StartAsync();
        _connectionString = await Infrastructure.GetConnectionStringAsync("db");
        await Infrastructure.ResourceNotifications.WaitForResourceHealthyAsync("db-server");
        await Infrastructure.ResourceNotifications.WaitForResourceHealthyAsync("identity");
    }

    public async ValueTask DisposeAsync()
    {
        await Infrastructure.DisposeAsync();
        UserTokenService.Dispose();
        ServiceTokenService.Dispose();
        KeycloakAdminTokenService.Dispose();
    }

    public async Task<DbContextConnectionStrings> CreateDatabaseAsync(string database)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
        await createCmd.ExecuteNonQueryAsync();
        var readDbContext = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = database,
            Username = "readonly_user"
        };
        var writeDbContext = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = database
        };
        return new DbContextConnectionStrings
        {
            ReadDbContext = readDbContext,
            WriteDbContext = writeDbContext
        };
    }

}
