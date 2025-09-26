using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Shared.Infrastructure.Options;

namespace Shared.Presentation.Transformers;

public sealed class SecuritySchemeDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly KeycloakOptions _keycloakOptions;

    public SecuritySchemeDocumentTransformer(IOptions<KeycloakOptions> keycloakOptions)
    {
        _keycloakOptions = keycloakOptions.Value;
    }

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var requirements = new Dictionary<string, IOpenApiSecurityScheme>
        {
            [Constants.SecuritySchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{_keycloakOptions.Issuer}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{_keycloakOptions.Issuer}/protocol/openid-connect/token"),
                        RefreshUrl = new Uri($"{_keycloakOptions.Issuer}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID Connect scope" }
                        }
                    }
                }
            }
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = requirements;

        return Task.CompletedTask;
    }
}