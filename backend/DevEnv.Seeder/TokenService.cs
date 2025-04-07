using Microsoft.Extensions.Options;
using SharedKernel.Options;

namespace DevEnv.Seeder;

public abstract class TokenService
{
    private readonly SemaphoreSlim _semaphore;
    protected readonly KeycloakOptions KeycloakOptions;
    protected readonly HttpClient HttpClient;

    private string _currentToken;
    private DateTime _expirationTime;

    public TokenService(
        IOptions<KeycloakOptions> options,
        HttpClient httpClient
    )
    {
        _semaphore = new(1, 1);
        KeycloakOptions = options.Value;
        HttpClient = httpClient;
    }

    public async Task<string> GetAccessTokenAsync()
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

    protected abstract Task<TokenResponse> RequestNewTokenAsync();
}