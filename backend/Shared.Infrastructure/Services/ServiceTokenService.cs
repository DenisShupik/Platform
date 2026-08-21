using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Options;

namespace Shared.Infrastructure.Services;

public sealed class ServiceTokenService : IDisposable
{
    public sealed class Handler(ServiceTokenService tokenService) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await tokenService.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                tokenService.InvalidateAccessToken(token);
            }

            return response;
        }
    }

    private readonly ClientCredentialsTokenService _tokenService;

    public ServiceTokenService(
        IOptions<KeycloakOptions> keycloakOptions,
        IOptions<ServiceAccountOptions> serviceAccountOptions)
    {
        var identity = keycloakOptions.Value;
        var serviceAccount = serviceAccountOptions.Value;
        _tokenService = new ClientCredentialsTokenService(
            identity.Issuer,
            serviceAccount.ClientId,
            serviceAccount.ClientSecret);
    }

    internal ServiceTokenService(
        IOptions<KeycloakOptions> keycloakOptions,
        IOptions<ServiceAccountOptions> serviceAccountOptions,
        HttpMessageHandler tokenHttpMessageHandler)
    {
        var identity = keycloakOptions.Value;
        var serviceAccount = serviceAccountOptions.Value;
        _tokenService = new ClientCredentialsTokenService(
            identity.Issuer,
            serviceAccount.ClientId,
            serviceAccount.ClientSecret,
            tokenHttpMessageHandler);
    }

    private Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
        _tokenService.GetAccessTokenAsync(cancellationToken);

    private void InvalidateAccessToken(string token) =>
        _tokenService.InvalidateAccessToken(token);

    public void Dispose() => _tokenService.Dispose();
}
