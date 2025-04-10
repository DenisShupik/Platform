using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SharedKernel.Options;

namespace DevEnv.Seeder;

public sealed class UserTokenService : TokenService
{
    public UserTokenService(IOptions<KeycloakOptions> options, HttpClient httpClient) : base(options, httpClient)
    {
    }
    
    protected override async Task<TokenResponse> RequestNewTokenAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{KeycloakOptions.Issuer}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = KeycloakOptions.Audience,
                ["username"]= "user1",
                ["password"]= "12345678"
            })
        };

        var response = await HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TokenResponse>();
    }
}