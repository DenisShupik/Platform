using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SharedKernel.Presentation.Options;
using SharedKernel.Tests.Dtos;

namespace SharedKernel.Tests.Services;

public sealed class ServiceTokenService : IDisposable
{
    public sealed class Handler : DelegatingHandler
    {
        private readonly ServiceTokenService _tokenService;

        public Handler(ServiceTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var token = await _tokenService.GetAccessTokenAsync();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }

    private readonly KeycloakOptions _keycloakOptions;
    private readonly HttpClient _httpClient;

    private string _currentToken;
    private readonly SemaphoreSlim _semaphore;

    private DateTime _expirationTime;

    public ServiceTokenService(
        IOptions<KeycloakOptions> options
    )
    {
        _keycloakOptions = options.Value;
        _httpClient = new HttpClient();
        _semaphore = new SemaphoreSlim(1, 1);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (!TokenNeedsRefresh())
        {
            return _currentToken;
        }

        try
        {
            await _semaphore.WaitAsync();

            if (!TokenNeedsRefresh())
            {
                return _currentToken;
            }

            var tokenResponse = await RequestNewTokenAsync();
            _currentToken = tokenResponse.AccessToken;
            _expirationTime = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            return _currentToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private bool TokenNeedsRefresh()
    {
        return string.IsNullOrEmpty(_currentToken) ||
               DateTime.UtcNow.AddSeconds(30) >= _expirationTime;
    }

    private async Task<TokenResponse> RequestNewTokenAsync()
    {
        var request =
            new HttpRequestMessage(HttpMethod.Post, $"{_keycloakOptions.Issuer}/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = "app-service",
                    ["client_secret"] = "4MZ1td4U3CSSqjwrOkgLRukvEcEe9eeN",
                })
            };

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        _semaphore.Dispose();
    }
}