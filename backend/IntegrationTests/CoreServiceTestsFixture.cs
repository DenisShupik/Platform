using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Shared.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using Shared.Tests.Dtos;
using Shared.Tests.Services;
using TUnit.Core.Interfaces;

namespace IntegrationTests;

public sealed class CoreServiceTestsFixture<T> : WebApplicationFactory<CoreService.Program>, IAsyncInitializer
{
    public readonly string TestModeratorUsername = typeof(T).Name + "_test_moderator";
    public UserId TestModeratorUserId;
    public readonly string TestUsername = typeof(T).Name + "_test_user";
    public UserId TestUserId;

    [ClassDataSource<InfrastructureFixture>(Shared = SharedType.PerAssembly)]
    public required InfrastructureFixture InfrastructureFixture { get; init; }

    private DbContextConnectionStrings? _connectionStrings;

    public async Task InitializeAsync()
    {
        _connectionStrings = await InfrastructureFixture.CreateDatabaseAsync($"{typeof(T).Name.ToLower()}_platform_db");
        
        var httpClientHandler = new HttpClientHandler();
        var serviceTokenHandler = new ServiceTokenService.Handler(InfrastructureFixture.ServiceTokenService)
        {
            InnerHandler = httpClientHandler
        };
        using var httpClient = new HttpClient(serviceTokenHandler);

        var keycloakAdminClient =
            new KeycloakAdminClient(httpClient,
                new OptionsWrapper<KeycloakOptions>(InfrastructureFixture.KeycloakOptions));

        TestUserId = await keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
        {
            Username = TestUsername,
            FirstName = "Иван",
            LastName = "Иванов",
            Email = $"{TestUsername}@app.com",
            Enabled = true,
            Credentials =
            [
                new()
                {
                    Type = "password",
                    Value = "12345678",
                    Temporary = false
                }
            ]
        }, CancellationToken.None);

        TestModeratorUserId = await keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
        {
            Username = TestModeratorUsername,
            FirstName = "Петр",
            LastName = "Петров",
            Email = $"{TestModeratorUsername}@app.com",
            Enabled = true,
            Credentials =
            [
                new()
                {
                    Type = "password",
                    Value = "12345678",
                    Temporary = false
                }
            ]
        }, CancellationToken.None);

        AssignRoleToUserRequestBody requestBody =
            [new() { ClientId = Guid.Parse("d2c62a5e-c2e2-419b-a176-cc45be86d1eb"), Role = nameof(Role.Moderator) }];

        await keycloakAdminClient.AssignRoleToUserAsync(TestModeratorUserId, requestBody, CancellationToken.None);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(_connectionStrings);

        builder.UseEnvironment("Development");
        builder.UseSetting("KeycloakOptions:MetadataAddress", InfrastructureFixture.KeycloakOptions.MetadataAddress);
        builder.UseSetting("KeycloakOptions:Issuer", InfrastructureFixture.KeycloakOptions.Issuer);
        builder.UseSetting("KeycloakOptions:Audience", InfrastructureFixture.KeycloakOptions.Audience);
        builder.UseSetting("KeycloakOptions:Realm", InfrastructureFixture.KeycloakOptions.Realm);
        builder.UseSetting("KeycloakOptions:ServiceClientId", InfrastructureFixture.KeycloakOptions.ServiceClientId);
        builder.UseSetting("KeycloakOptions:ServiceClientSecret",
            InfrastructureFixture.KeycloakOptions.ServiceClientSecret);
        builder.UseSetting("RabbitMqOptions:Host", InfrastructureFixture.RabbitMqOptions.Host);
        builder.UseSetting("RabbitMqOptions:Username", InfrastructureFixture.RabbitMqOptions.Username);
        builder.UseSetting("RabbitMqOptions:Password", InfrastructureFixture.RabbitMqOptions.Password);
        builder.UseSetting("CoreServiceOptions:ReadonlyConnectionString",
            _connectionStrings.ReadDbContext.ConnectionString);
        builder.UseSetting("CoreServiceOptions:WritableConnectionString",
            _connectionStrings.WriteDbContext.ConnectionString);
    }

    public CoreServiceClient GetCoreServiceClient(string? username = null)
    {
        HttpClient client;
        if (username == null)
        {
            client = CreateClient();
        }
        else
        {
            var handler = new UserTokenService.Handler(InfrastructureFixture.UserTokenService, () => username);
            client = CreateDefaultClient(handler);
        }

        return new CoreServiceClient(client);
    }
}