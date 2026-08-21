using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Options;
using Shared.Infrastructure.Services;
using Shared.Tests.Dtos;
using Shared.Tests.Services;
using TUnit.Core.Interfaces;

namespace IntegrationTests;

public sealed class UserServiceTestsFixture<T> :
    WebApplicationFactory<UserService.Program>,
    IAsyncInitializer
{
    public readonly string TestUsername = $"{typeof(T).Name.ToLowerInvariant()}_locale_user";
    public UserId TestUserId;

    [ClassDataSource<InfrastructureFixture>(Shared = SharedType.PerAssembly)]
    public required InfrastructureFixture InfrastructureFixture { get; init; }

    private DbContextConnectionStrings? _connectionStrings;

    public async Task InitializeAsync()
    {
        _connectionStrings = await InfrastructureFixture.CreateDatabaseAsync(
            $"{typeof(T).Name.ToLowerInvariant()}_user_service_platform_db");

        using var httpClient = new HttpClient(new KeycloakAdminTokenService.Handler(
            InfrastructureFixture.KeycloakAdminTokenService)
        {
            InnerHandler = new HttpClientHandler()
        });
        var keycloakAdminClient = new KeycloakAdminClient(
            httpClient,
            new OptionsWrapper<KeycloakOptions>(InfrastructureFixture.KeycloakOptions));

        TestUserId = await keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
        {
            Username = TestUsername,
            FirstName = "Preference",
            LastName = "Tester",
            Email = $"{TestUsername}@app.com",
            Enabled = true,
            Credentials =
            [
                new CreateUserRequestBody.Credential
                {
                    Type = "password",
                    Value = "12345678",
                    Temporary = false
                }
            ]
        }, CancellationToken.None);

        using var bootstrapClient = CreateClient();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(_connectionStrings);

        builder.UseEnvironment("Development");
        builder.UseSetting("KeycloakOptions:MetadataAddress", InfrastructureFixture.KeycloakOptions.MetadataAddress);
        builder.UseSetting("KeycloakOptions:Issuer", InfrastructureFixture.KeycloakOptions.Issuer);
        builder.UseSetting("KeycloakOptions:Audience", InfrastructureFixture.KeycloakOptions.Audience);
        builder.UseSetting("KeycloakOptions:InternalAudience",
            InfrastructureFixture.KeycloakOptions.InternalAudience);
        builder.UseSetting("KeycloakOptions:Realm", InfrastructureFixture.KeycloakOptions.Realm);
        builder.UseSetting("InternalApiOptions:CoreServiceClientId",
            InfrastructureFixture.InternalApiOptions.CoreServiceClientId);
        builder.UseSetting("InternalApiOptions:NotificationServiceClientId",
            InfrastructureFixture.InternalApiOptions.NotificationServiceClientId);
        builder.UseSetting("InternalApiOptions:ProvisioningServiceClientId",
            InfrastructureFixture.InternalApiOptions.ProvisioningServiceClientId);
        builder.UseSetting("KeycloakAdminOptions:ClientId",
            InfrastructureFixture.KeycloakAdminOptions.ClientId);
        builder.UseSetting("KeycloakAdminOptions:ClientSecret",
            InfrastructureFixture.KeycloakAdminOptions.ClientSecret);
        builder.UseSetting("RabbitMqOptions:Host", InfrastructureFixture.RabbitMqOptions.Host);
        builder.UseSetting("RabbitMqOptions:Username", InfrastructureFixture.RabbitMqOptions.Username);
        builder.UseSetting("RabbitMqOptions:Password", InfrastructureFixture.RabbitMqOptions.Password);
        builder.UseSetting("UserServiceOptions:ReadonlyConnectionString",
            _connectionStrings.ReadDbContext.ConnectionString);
        builder.UseSetting("UserServiceOptions:WritableConnectionString",
            _connectionStrings.WriteDbContext.ConnectionString);
    }

    public HttpClient GetAuthenticatedClient(string locale)
    {
        var handler = new UserTokenService.Handler(
            InfrastructureFixture.UserTokenService,
            () => TestUsername);
        var client = CreateDefaultClient(handler);
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(locale);
        return client;
    }

    public async Task<string?> GetKeycloakUserLocaleAsync(CancellationToken cancellationToken)
    {
        using var httpClient = new HttpClient(new KeycloakAdminTokenService.Handler(
            InfrastructureFixture.KeycloakAdminTokenService)
        {
            InnerHandler = new HttpClientHandler()
        });
        var keycloakAdminClient = new KeycloakAdminClient(
            httpClient,
            new OptionsWrapper<KeycloakOptions>(InfrastructureFixture.KeycloakOptions));
        return await keycloakAdminClient.GetUserLocaleAsync(TestUserId, cancellationToken);
    }

    public async Task<string> GetFreshUserAccessTokenAsync(CancellationToken cancellationToken)
    {
        using var tokenService = new UserTokenService(
            new OptionsWrapper<KeycloakOptions>(InfrastructureFixture.KeycloakOptions));
        return await tokenService.GetAccessTokenAsync(TestUsername, cancellationToken);
    }
}
