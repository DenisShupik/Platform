using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Presentation.Authorization;
using Shared.Presentation.Extensions;

namespace IntegrationTests.Tests;

public sealed class AuthenticationPolicyTests
{
    [Test]
    public async Task PublicAndInternalPolicies_SeparateUserAndWorkloadClients()
    {
        await using var services = CreateServices();
        var authorization = services.GetRequiredService<IAuthorizationService>();
        var publicPrincipal = Principal("app-user", "app-user");
        var internalPrincipal = Principal("notification-service", "app-internal");

        await Assert.That((await authorization.AuthorizeAsync(
            publicPrincipal, null, AuthenticationPolicies.PublicApi)).Succeeded).IsTrue();
        await Assert.That((await authorization.AuthorizeAsync(
            publicPrincipal, null, AuthenticationPolicies.InternalApi)).Succeeded).IsFalse();
        await Assert.That((await authorization.AuthorizeAsync(
            internalPrincipal, null, AuthenticationPolicies.InternalApi)).Succeeded).IsTrue();
        await Assert.That((await authorization.AuthorizeAsync(
            internalPrincipal, null, AuthenticationPolicies.NotificationServiceInternalApi)).Succeeded).IsTrue();
        await Assert.That((await authorization.AuthorizeAsync(
            internalPrincipal, null, AuthenticationPolicies.ProvisioningServiceInternalApi)).Succeeded).IsFalse();
        await Assert.That((await authorization.AuthorizeAsync(
            internalPrincipal, null, AuthenticationPolicies.PublicApi)).Succeeded).IsFalse();
    }

    [Test]
    public async Task InternalPolicy_AcceptsKeycloakClientIdClaim()
    {
        await using var services = CreateServices();
        var authorization = services.GetRequiredService<IAuthorizationService>();
        var identity = new ClaimsIdentity(
            [new Claim("client_id", "core-service"), new Claim("aud", "app-internal")],
            "test");

        var result = await authorization.AuthorizeAsync(
            new ClaimsPrincipal(identity), null, AuthenticationPolicies.InternalApi);

        await Assert.That(result.Succeeded).IsTrue();
    }

    private static ServiceProvider CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["KeycloakOptions:MetadataAddress"] = "https://identity.example/.well-known/openid-configuration",
                ["KeycloakOptions:Issuer"] = "https://identity.example/realms/platform",
                ["KeycloakOptions:Audience"] = "app-user",
                ["KeycloakOptions:InternalAudience"] = "app-internal",
                ["KeycloakOptions:Realm"] = "platform",
                ["InternalApiOptions:CoreServiceClientId"] = "core-service",
                ["InternalApiOptions:NotificationServiceClientId"] = "notification-service",
                ["InternalApiOptions:ProvisioningServiceClientId"] = "dev-provisioner"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.RegisterAuthenticationSchemes(configuration);
        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal Principal(string authorizedParty, string audience) =>
        new(new ClaimsIdentity(
            [new Claim("azp", authorizedParty), new Claim("aud", audience)],
            "test"));
}
