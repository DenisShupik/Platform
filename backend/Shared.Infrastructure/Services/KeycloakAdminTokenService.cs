using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;

namespace Shared.Infrastructure.Services;

public sealed class KeycloakAdminTokenService : IDisposable
{
    public sealed class Handler(KeycloakAdminTokenService tokenService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await tokenService._tokenService.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                tokenService._tokenService.InvalidateAccessToken(token);
            }

            return response;
        }
    }

    private readonly ClientCredentialsTokenService _tokenService;

    public KeycloakAdminTokenService(
        IOptions<KeycloakOptions> keycloakOptions,
        IOptions<KeycloakAdminOptions> adminOptions)
    {
        _tokenService = new ClientCredentialsTokenService(
            keycloakOptions.Value.Issuer,
            adminOptions.Value.ClientId,
            adminOptions.Value.ClientSecret);
    }

    public void Dispose() => _tokenService.Dispose();
}
