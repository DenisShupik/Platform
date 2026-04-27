using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Dtos;
using Shared.Infrastructure.Options;

namespace Shared.Infrastructure.Services;

public sealed class ServiceTokenService : IDisposable
{
    public sealed class Handler : DelegatingHandler
    {
        private readonly ServiceTokenService _tokenService;

        public Handler(ServiceTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

    private record TokenData(string Token, DateTime ExpiresAt);

    private TokenData? _cachedToken;
    private readonly HttpClient _httpClient;
    private readonly FormUrlEncodedContent _tokenRequestContent;
    private readonly SemaphoreSlim _semaphore;

    public ServiceTokenService(IOptions<KeycloakOptions> options)
    {
        var keycloakOptions = options.Value;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{keycloakOptions.Issuer}/protocol/openid-connect/")
        };
        _semaphore = new SemaphoreSlim(1, 1);
        _tokenRequestContent = new FormUrlEncodedContent([
            new("grant_type", "client_credentials"),
            new("client_id", keycloakOptions.ServiceClientId),
            new("client_secret", keycloakOptions.ServiceClientSecret)
        ]);
        _cachedToken = null;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (TryGetValidToken(out var token))
        {
            return token;
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken);

            if (TryGetValidToken(out token))
            {
                return token;
            }

            var tokenResponse = await RequestNewTokenAsync(cancellationToken);
            _cachedToken = new TokenData(tokenResponse.AccessToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn));

            return tokenResponse.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool TryGetValidToken([NotNullWhen(true)] out string? token)
    {
        var data = _cachedToken;
        if (data != null && DateTime.UtcNow.AddSeconds(30) < data.ExpiresAt)
        {
            token = data.Token;
            return true;
        }

        token = null;
        return false;
    }

    private async Task<TokenResponse> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        // HINT: using не нужен, так как иначе будет вызван Content.Dispose,
        // но мы единожды аллоцируем Content и переиспользуем
        var response = await _httpClient.PostAsync("token", _tokenRequestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _semaphore.Dispose();
        _tokenRequestContent.Dispose();
    }
}