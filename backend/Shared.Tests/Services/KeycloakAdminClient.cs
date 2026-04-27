using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Shared.Domain.ValueObjects;
using Shared.Tests.Dtos;
using Shared.Infrastructure.Options;

namespace Shared.Tests.Services;

public sealed class KeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;

    public KeycloakAdminClient(HttpClient httpClient, IOptions<KeycloakOptions> keycloakOptions)
    {
        _httpClient = httpClient;
        var builder = new UriBuilder(keycloakOptions.Value.Issuer);
        builder.Path = $"/admin{builder.Path}/";
        var modifiedUri = builder.Uri;
        _httpClient.BaseAddress = modifiedUri;
        _clientId = keycloakOptions.Value.Audience;
    }

    public async Task<UserId> CreateUserAsync(CreateUserRequestBody requestBody, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync("users", requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();
        var location = response.Headers.Location;
        var value = location?.Segments[^1];
        return UserId.From(Guid.Parse(value!));
    }

    public async Task AssignRoleToUserAsync(UserId userId, AssignRoleToUserRequestBody requestBody,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.PostAsJsonAsync($"users/{userId}/role-mappings/clients/{_clientId}",
            requestBody,
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}