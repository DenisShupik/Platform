using System.Diagnostics.CodeAnalysis;
using Shared.Infrastructure.Dtos;

namespace Shared.Infrastructure.Services;

internal sealed class ClientCredentialsTokenService : IDisposable
{
    private sealed record TokenData(string Token, DateTime ExpiresAt);

    private TokenData? _cachedToken;
    private readonly HttpClient _httpClient;
    private readonly FormUrlEncodedContent _tokenRequestContent;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public ClientCredentialsTokenService(
        string issuer,
        string clientId,
        string clientSecret,
        HttpMessageHandler? tokenHttpMessageHandler = null)
    {
        _httpClient = tokenHttpMessageHandler is null
            ? new HttpClient()
            : new HttpClient(tokenHttpMessageHandler);
        _httpClient.BaseAddress = new Uri($"{issuer}/protocol/openid-connect/");
        _tokenRequestContent = new FormUrlEncodedContent([
            new("grant_type", "client_credentials"),
            new("client_id", clientId),
            new("client_secret", clientSecret)
        ]);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (TryGetValidToken(out var token)) return token;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (TryGetValidToken(out token)) return token;

            var tokenResponse = await RequestNewTokenAsync(cancellationToken);
            _cachedToken = new TokenData(
                tokenResponse.AccessToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn));

            return tokenResponse.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void InvalidateAccessToken(string token)
    {
        var cachedToken = _cachedToken;
        if (cachedToken?.Token == token)
        {
            Interlocked.CompareExchange(ref _cachedToken, null, cachedToken);
        }
    }

    private bool TryGetValidToken([NotNullWhen(true)] out string? token)
    {
        var data = _cachedToken;
        if (data is not null && DateTime.UtcNow.AddSeconds(30) < data.ExpiresAt)
        {
            token = data.Token;
            return true;
        }

        token = null;
        return false;
    }

    private async Task<TokenResponse> RequestNewTokenAsync(CancellationToken cancellationToken)
    {
        // Content is allocated once and reused for every token refresh.
        using var response = await _httpClient.PostAsync("token", _tokenRequestContent, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
               ?? throw new InvalidOperationException("Keycloak returned an empty client-credentials response.");
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _semaphore.Dispose();
        _tokenRequestContent.Dispose();
    }
}
