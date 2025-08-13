using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SharedKernel.Infrastructure.Options;
using SharedKernel.Tests.Dtos;
using UserService.Domain.ValueObjects;

namespace SharedKernel.Tests.Services;

public sealed class KeycloakAdminClient
{
    private readonly HttpClient _httpClient;

    public KeycloakAdminClient(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    {
        _httpClient = httpClient;
        var builder = new UriBuilder(keycloakOptions.Value.Issuer);
        builder.Path = $"/admin{builder.Path}/";
        var modifiedUri = builder.Uri;
        _httpClient.BaseAddress = modifiedUri;
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