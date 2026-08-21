using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Options;
using Shared.Tests.Dtos;

namespace Shared.Tests.Services;

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

    public async Task<string?> GetUserLocaleAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync($"users/{userId}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var user = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        return user?["attributes"]?["locale"]?[0]?.GetValue<string>();
    }

}
