using System.Net.Http.Headers;

namespace DevEnv.Seeder;

public sealed class AuthenticationMessageHandler<T> : DelegatingHandler
    where T : TokenService
{
    private readonly T _tokenService;

    public AuthenticationMessageHandler(T tokenService)
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