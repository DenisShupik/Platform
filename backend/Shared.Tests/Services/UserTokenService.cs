using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Dtos;
using Shared.Infrastructure.Options;

namespace Shared.Tests.Services;

public sealed class UserTokenService : IDisposable
{
    public sealed class Handler : DelegatingHandler
    {
        private readonly UserTokenService _userService;
        private readonly Func<string> _userSelector;

        public Handler(UserTokenService userService, Func<string> userSelector)
        {
            _userService = userService;
            _userSelector = userSelector;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var token = await _userService.GetAccessTokenAsync(_userSelector(), cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class CachedToken : IDisposable
    {
        private record TokenData(string Token, DateTime ExpiresAt);

        private TokenData? _data;

        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public bool TryGetValidToken([NotNullWhen(true)] out string? token)
        {
            var data = _data;
            if (data != null && DateTime.UtcNow.AddSeconds(30) < data.ExpiresAt)
            {
                token = data.Token;
                return true;
            }

            token = null;
            return false;
        }

        public void SetToken(string token, int expiresIn)
        {
            _data = new TokenData(token, DateTime.UtcNow.AddSeconds(expiresIn));
        }

        public void Dispose() => Semaphore.Dispose();
    }

    private readonly string _audience;
    private readonly HttpClient _httpClient;

    private readonly ConcurrentDictionary<string, CachedToken> _cachedTokens;

    public UserTokenService(IOptions<KeycloakOptions> options)
    {
        var keycloakOptions = options.Value;
        _audience = keycloakOptions.Audience;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri($"{keycloakOptions.Issuer}/protocol/openid-connect/")
        };
        _cachedTokens = new ConcurrentDictionary<string, CachedToken>();
    }

    public async Task<string> GetAccessTokenAsync(string username, CancellationToken cancellationToken)
    {
        var cached = _cachedTokens.GetOrAdd(username, _ => new CachedToken());

        if (cached.TryGetValidToken(out var token))
        {
            return token;
        }

        await cached.Semaphore.WaitAsync(cancellationToken);
        try
        {
            if (cached.TryGetValidToken(out token))
            {
                return token;
            }

            var tokenResponse = await RequestNewTokenAsync(username, cancellationToken);
            cached.SetToken(tokenResponse.AccessToken, tokenResponse.ExpiresIn);

            return tokenResponse.AccessToken;
        }
        finally
        {
            cached.Semaphore.Release();
        }
    }

    private async Task<TokenResponse> RequestNewTokenAsync(string username, CancellationToken cancellationToken)
    {
        using var content = new FormUrlEncodedContent([
            new("grant_type", "password"),
            new("client_id", _audience),
            new("username", username),
            new("password", "12345678")
        ]);

        using var response = await _httpClient.PostAsync("token", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize token response");
    }

    public void Dispose()
    {
        _httpClient.Dispose();

        foreach (var cached in _cachedTokens.Values)
        {
            cached.Dispose();
        }
    }
}