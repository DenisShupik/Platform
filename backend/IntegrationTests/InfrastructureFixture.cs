using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using SharedKernel.Domain.ValueObjects;
using SharedKernel.Presentation.Options;
using SharedKernel.Tests.Dtos;
using SharedKernel.Tests.Services;
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

    public InfrastructureFixture()
    {
        KeycloakOptions = new KeycloakOptions
        {
            MetadataAddress = "http://localhost:8080/realms/app-test/.well-known/openid-configuration",
            Issuer = "http://localhost:8080/realms/app-test",
            Audience = "app-test-user",
            Realm = "app-test"
        };

        Infrastructure = DistributedApplicationTestingBuilder.CreateAsync<Projects.DevEnv>([
                "DcpPublisher:RandomizePorts=false",
                "Seeding=false",
                "DisableServices=true",
                $"KeycloakOptions:MetadataAddress={KeycloakOptions.MetadataAddress}",
                $"KeycloakOptions:Issuer={KeycloakOptions.Issuer}",
                $"KeycloakOptions:Audience={KeycloakOptions.Audience}",
                $"KeycloakOptions:Realm={KeycloakOptions.Realm}",
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

    public NpgsqlConnectionStringBuilder CreateDatabase(string database)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        using var createCmd = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", connection);
        createCmd.ExecuteNonQuery();
        var builder = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            Database = database
        };
        return builder;
    }
}