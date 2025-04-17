using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SharedKernel.Presentation.Options;
using SharedKernel.Tests.Dtos;

namespace SharedKernel.Tests.Services;

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

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var token = await _userService.GetAccessTokenAsync(_userSelector());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class CachedToken
    {
        public string Token { get; set; }
        public DateTime ExpirationTime { get; set; }
        public SemaphoreSlim Semaphore { get; }

        public CachedToken()
        {
            Semaphore = new SemaphoreSlim(1, 1);
        }
    }

    private readonly KeycloakOptions _keycloakOptions;
    private readonly HttpClient _httpClient;

    private readonly ConcurrentDictionary<string, CachedToken> _cachedTokens;

    public UserTokenService(
        IOptions<KeycloakOptions> options
    )
    {
        _keycloakOptions = options.Value;
        _httpClient = new HttpClient();
        _cachedTokens = new ConcurrentDictionary<string, CachedToken>();
    }


    public async Task<string> GetAccessTokenAsync(string username)
    {
        var cached = _cachedTokens.GetOrAdd(username, _ => new CachedToken());

        if (!TokenNeedsRefresh(cached))
        {
            return cached.Token;
        }

        await cached.Semaphore.WaitAsync();
        try
        {
            if (!TokenNeedsRefresh(cached))
            {
                return cached.Token;
            }

            var tokenResponse = await RequestNewTokenAsync(username);
            cached.Token = tokenResponse.AccessToken;
            cached.ExpirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return cached.Token;
        }
        finally
        {
            cached.Semaphore.Release();
        }
    }

    private bool TokenNeedsRefresh(CachedToken cached)
    {
        return string.IsNullOrEmpty(cached.Token) ||
               DateTime.UtcNow.AddSeconds(30) >= cached.ExpirationTime;
    }

    private async Task<TokenResponse> RequestNewTokenAsync(string username)
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_keycloakOptions.Issuer}/protocol/openid-connect/token"
        )
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = _keycloakOptions.Audience,
                ["username"] = username,
                ["password"] = "12345678"
            })
        };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}