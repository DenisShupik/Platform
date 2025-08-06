using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Infrastructure.Services;
using SharedKernel.Tests.Dtos;
using SharedKernel.Tests.Services;
using UserService.Domain.ValueObjects;

namespace IntegrationTests;

public sealed class CoreServiceTestsFixture<T> : WebApplicationFactory<CoreService.Program>
{
    private readonly InfrastructureFixture _infrastructureFixture;
    public readonly string TestUsername = typeof(T).Name + "_test_user";
    public UserId TestUserId;

    public CoreServiceTestsFixture(
        InfrastructureFixture fixture
    )
    {
        _infrastructureFixture = fixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionStringBuilder =
            _infrastructureFixture.CreateDatabase($"{typeof(T).Name.ToLower()}_platform_db");

        builder.UseEnvironment("Development");
        builder.UseSetting("KeycloakOptions:MetadataAddress", _infrastructureFixture.KeycloakOptions.MetadataAddress);
        builder.UseSetting("KeycloakOptions:Issuer", _infrastructureFixture.KeycloakOptions.Issuer);
        builder.UseSetting("KeycloakOptions:Audience", _infrastructureFixture.KeycloakOptions.Audience);
        builder.UseSetting("KeycloakOptions:Realm", _infrastructureFixture.KeycloakOptions.Realm);
        builder.UseSetting("KeycloakOptions:ServiceClientId", _infrastructureFixture.KeycloakOptions.ServiceClientId);
        builder.UseSetting("KeycloakOptions:ServiceClientSecret",
            _infrastructureFixture.KeycloakOptions.ServiceClientSecret);
        builder.UseSetting("RabbitMqOptions:Host", _infrastructureFixture.RabbitMqOptions.Host);
        builder.UseSetting("RabbitMqOptions:Username", _infrastructureFixture.RabbitMqOptions.Username);
        builder.UseSetting("RabbitMqOptions:Password", _infrastructureFixture.RabbitMqOptions.Password);
        builder.UseSetting("CoreServiceOptions:ReadonlyConnectionString",
            connectionStringBuilder.ReadonlyDbContext.ConnectionString);
        builder.UseSetting("CoreServiceOptions:WritableConnectionString",
            connectionStringBuilder.WritableDbContext.ConnectionString);

        var httpClientHandler = new HttpClientHandler();
        var serviceTokenHandler = new ServiceTokenService.Handler(_infrastructureFixture.ServiceTokenService)
        {
            InnerHandler = httpClientHandler
        };
        using var httpClient = new HttpClient(serviceTokenHandler);

        var keycloakAdminClient =
            new KeycloakAdminClient(httpClient,
                new OptionsWrapper<KeycloakOptions>(_infrastructureFixture.KeycloakOptions));

        // TODO: разобраться почему Keycloak проба /ready возвращает 200, но Keycloak не готов еще к обработке
        var repeat = true;
        while (repeat)
            try
            {
                TestUserId = keycloakAdminClient.CreateUserAsync(new CreateUserRequestBody
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
                }, CancellationToken.None).Result;
                repeat = false;
            }
            catch
            {
                // ignored
            }
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
            var handler = new UserTokenService.Handler(_infrastructureFixture.UserTokenService, () => username);
            client = CreateDefaultClient(handler);
        }

        return new CoreServiceClient(client);
    }
}