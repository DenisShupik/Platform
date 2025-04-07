using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SharedKernel.Options;

namespace DevEnv.Seeder;

public sealed class ServiceTokenService : TokenService
{
    public ServiceTokenService(IOptions<KeycloakOptions> options, HttpClient httpClient) : base(options, httpClient)
    {
    }
    
    protected override async Task<TokenResponse> RequestNewTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{KeycloakOptions.Issuer}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "traveltell-service",
                ["client_secret"] = "4MZ1td4U3CSSqjwrOkgLRukvEcEe9eeN",
            })
        };

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }
}