using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Options;
using UserService.Application.Interfaces;

namespace UserService.Infrastructure.Services;

public sealed class KeycloakUserLocaleClient : IUserLocaleIdentityProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _usersEndpoint;

    public KeycloakUserLocaleClient(
        HttpClient httpClient,
        IOptions<KeycloakOptions> options)
    {
        var keycloakOptions = options.Value;
        _httpClient = httpClient;
        _usersEndpoint = $"admin/realms/{Uri.EscapeDataString(keycloakOptions.Realm)}/users";
    }

    public async Task<bool> ChangeLocaleAsync(
        UserId userId,
        Locale locale,
        CancellationToken cancellationToken)
    {
        try
        {
            return await ChangeLocaleCoreAsync(userId, locale, cancellationToken);
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            throw new HttpRequestException(
                "Keycloak returned an invalid user representation.",
                exception,
                HttpStatusCode.BadGateway);
        }
    }

    private async Task<bool> ChangeLocaleCoreAsync(
        UserId userId,
        Locale locale,
        CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(userId, cancellationToken);
        if (user is null) return false;
        if (HasLocale(user, locale)) return true;

        var attributes = user["attributes"] switch
        {
            null => new JsonObject(),
            JsonObject value => value,
            _ => throw new InvalidOperationException("Keycloak returned invalid user attributes.")
        };
        user["attributes"] = attributes;
        attributes["locale"] = new JsonArray(locale.Value);

        using var response = await _httpClient.PutAsJsonAsync(
            $"{_usersEndpoint}/{userId}",
            user,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        return true;
    }

    private async Task<JsonObject?> GetUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"{_usersEndpoint}/{userId}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken)
               ?? throw new InvalidOperationException("Keycloak returned an empty user representation.");
    }

    private static bool HasLocale(JsonObject user, Locale locale) =>
        user["attributes"] is JsonObject attributes &&
        attributes["locale"] is JsonArray { Count: 1 } locales &&
        locales[0]?.GetValue<string>() == locale.Value;
}
