using System.Net.Http.Json;
using SharedKernel.Domain.ValueObjects;

namespace DevEnv.Seeder;

public sealed class KeycloakClient
{
    private readonly HttpClient _httpClient;

    public KeycloakClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:8080/admin/realms/traveltell-dev/");
    }

    public async Task<UserId> CreateUserAsync(CreateUserRequestBody requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("users", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        var value = location?.Segments[^1];
        return UserId.From(Guid.Parse(value!));
    }
}