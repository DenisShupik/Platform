using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Options;
using Shared.Presentation.Authorization;

namespace Shared.Presentation.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterAuthenticationSchemes(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .RegisterOptions<KeycloakOptions, KeycloakOptionsValidator>(configuration)
            .RegisterOptions<InternalApiOptions, InternalApiOptionsValidator>(configuration)
            .AddAuthentication()
            .AddJwtBearer();

        var keycloakOptions = configuration.GetRequiredSection(nameof(KeycloakOptions)).Get<KeycloakOptions>();
        ArgumentNullException.ThrowIfNull(keycloakOptions);
        var internalApiOptions = configuration.GetRequiredSection(nameof(InternalApiOptions))
            .Get<InternalApiOptions>();
        ArgumentNullException.ThrowIfNull(internalApiOptions);

        var internalClientIds = new HashSet<string>(StringComparer.Ordinal)
        {
            internalApiOptions.CoreServiceClientId,
            internalApiOptions.NotificationServiceClientId,
            internalApiOptions.ProvisioningServiceClientId
        };

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthenticationPolicies.PublicApi, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        GetAuthorizedParty(context.User) == keycloakOptions.Audience &&
                        HasAudience(context.User, keycloakOptions.Audience)))
            .AddPolicy(AuthenticationPolicies.InternalApi, policy =>
                policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        GetAuthorizedParty(context.User) is { } clientId &&
                        internalClientIds.Contains(clientId) &&
                        HasAudience(context.User, keycloakOptions.InternalAudience)))
            .AddPolicy(AuthenticationPolicies.CoreServiceInternalApi, policy =>
                RequireInternalClient(policy, keycloakOptions, internalApiOptions.CoreServiceClientId))
            .AddPolicy(AuthenticationPolicies.NotificationServiceInternalApi, policy =>
                RequireInternalClient(policy, keycloakOptions, internalApiOptions.NotificationServiceClientId))
            .AddPolicy(AuthenticationPolicies.ProvisioningServiceInternalApi, policy =>
                RequireInternalClient(policy, keycloakOptions, internalApiOptions.ProvisioningServiceClientId));

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<KeycloakOptions>, IHostEnvironment>((options, keycloakOptions, environment) =>
            {
                options.MetadataAddress = keycloakOptions.Value.MetadataAddress;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = keycloakOptions.Value.Issuer,
                    ValidAudiences =
                    [
                        keycloakOptions.Value.Audience,
                        keycloakOptions.Value.InternalAudience
                    ],
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                        context.Exception is SecurityTokenExpiredException
                            ? throw context.Exception
                            : Task.CompletedTask
                };
            });

        return services;
    }

    private static string? GetAuthorizedParty(System.Security.Claims.ClaimsPrincipal principal) =>
        principal.FindFirst("client_id")?.Value ?? principal.FindFirst("azp")?.Value;

    private static bool HasAudience(System.Security.Claims.ClaimsPrincipal principal, string audience) =>
        principal.FindAll("aud").Any(claim => claim.Value == audience);

    private static void RequireInternalClient(
        Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder policy,
        KeycloakOptions keycloakOptions,
        string clientId) =>
        policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context =>
                GetAuthorizedParty(context.User) == clientId &&
                HasAudience(context.User, keycloakOptions.InternalAudience));
}
