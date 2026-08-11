using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.JsonWebTokens;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Errors;
using Shared.Presentation.Localization;

namespace IntegrationTests.Tests;

public sealed class UserLocaleLocalizationTests
{
    [ClassDataSource<UserServiceTestsFixture<UserLocaleLocalizationTests>>(Shared = SharedType.PerClass)]
    public required UserServiceTestsFixture<UserLocaleLocalizationTests> Fixture { get; init; }

    [Test]
    public async Task CurrentUser_CanChangeKeycloakLocaleSynchronously(CancellationToken cancellationToken)
    {
        using var client = Fixture.GetAuthenticatedClient(Locale.RussianCode);

        using var updateResponse = await client.PutAsJsonAsync(
            "api/users/current/locale",
            new { locale = Locale.RussianCode },
            cancellationToken);
        await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var keycloakLocale = await Fixture.GetKeycloakUserLocaleAsync(cancellationToken);
        await Assert.That(keycloakLocale).IsEqualTo(Locale.RussianCode);

        var freshAccessToken = await Fixture.GetFreshUserAccessTokenAsync(cancellationToken);
        var tokenLocale = new JsonWebToken(freshAccessToken).GetPayloadValue<string>("locale");
        await Assert.That(tokenLocale).IsEqualTo(Locale.RussianCode);
    }

    [Test]
    public async Task InvalidLocale_ReturnsRussianValidationWithStableCode(CancellationToken cancellationToken)
    {
        using var client = Fixture.GetAuthenticatedClient(Locale.RussianCode);
        using var response = await client.PutAsJsonAsync(
            "api/users/current/locale",
            new { locale = "de" },
            cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var root = json.RootElement;
        await Assert.That(root.GetProperty("code").GetString()).IsEqualTo("validation_failed");
        var localeError = root.GetProperty("errors").EnumerateObject().Single().Value;
        await Assert.That(localeError.GetProperty("code").GetString()).IsEqualTo("unsupported_locale");
        await Assert.That(localeError.GetProperty("message").GetString()).Contains("Локаль");
    }

    [Test]
    public async Task ChangeCurrentUserLocale_RequiresGeneratedAuthorizationMetadata(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", Locale.EnglishCode);

        using var response = await client.PutAsJsonAsync(
            "api/users/current/locale",
            new { locale = Locale.EnglishCode },
            cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ApiRequestWithoutLocale_IsRejectedWithoutLanguageFallback(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        using var response = await client.GetAsync("api/users", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotAcceptable);
        await Assert.That(response.Content.Headers.ContentLanguage).IsEmpty();
        var error = await response.Content.ReadFromJsonAsync<LocaleRequiredError>(cancellationToken);
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.SupportedLocales).IsEquivalentTo(Locale.SupportedCodes);
    }

    [Test]
    [Arguments("en;q=0")]
    [Arguments("ru-RU")]
    public async Task UnacceptableOrNonExactLocale_IsRejected(
        string acceptLanguage,
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", acceptLanguage);
        using var response = await client.GetAsync("api/users", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotAcceptable);
        var error = await response.Content.ReadFromJsonAsync<UnsupportedLocaleError>(cancellationToken);
        await Assert.That(error).IsNotNull();
        await Assert.That(error!.SupportedLocales).IsEquivalentTo(Locale.SupportedCodes);
    }

    [Test]
    public async Task LocaleSelection_UsesHighestPositiveSupportedQuality(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "Accept-Language",
            "de, en;q=0, ru;q=0.5");
        using var response = await client.GetAsync("api/users", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentLanguage).IsEquivalentTo([Locale.RussianCode]);
    }

    [Test]
    public async Task OpenApi_DeclaresLocaleAsExactEnRuEnum(CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var localeSchema = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(nameof(Locale));
        var values = localeSchema.GetProperty("enum")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();

        await Assert.That(values).IsEquivalentTo(Locale.SupportedCodes);
        await Assert.That(localeSchema.GetProperty("pattern").GetString()).IsEqualTo("^(en|ru)$");

        var operation = json.RootElement
            .GetProperty("paths")
            .GetProperty("/api/users/current/locale")
            .GetProperty("put");
        var acceptLanguage = operation.GetProperty("parameters")
            .EnumerateArray()
            .Single(parameter => parameter.GetProperty("name").GetString() == "Accept-Language");
        await Assert.That(acceptLanguage.GetProperty("required").GetBoolean()).IsTrue();
        var acceptedLocales = acceptLanguage.GetProperty("schema")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();
        await Assert.That(acceptedLocales).IsEquivalentTo(Locale.SupportedCodes);
        var responses = operation.GetProperty("responses");
        foreach (var (statusCode, contentType) in new[]
                 {
                     ("400", "application/problem+json"),
                     ("406", "application/json"),
                     ("413", "application/problem+json"),
                     ("500", "application/problem+json")
                 })
        {
            await Assert.That(responses.TryGetProperty(statusCode, out var documentedResponse)).IsTrue();
            await Assert.That(documentedResponse
                    .GetProperty("content")
                    .GetProperty(contentType)
                    .TryGetProperty("schema", out _))
                .IsTrue();
        }

        var contentLanguage = responses
            .GetProperty("204")
            .GetProperty("headers")
            .GetProperty("Content-Language");
        await Assert.That(contentLanguage.GetProperty("required").GetBoolean()).IsTrue();
        var responseLocales = contentLanguage.GetProperty("schema")
            .GetProperty("enum")
            .EnumerateArray()
            .Select(value => value.GetString()!)
            .ToArray();
        await Assert.That(responseLocales).IsEquivalentTo(Locale.SupportedCodes);
        await Assert.That(responses.GetProperty("406").TryGetProperty("headers", out _)).IsFalse();

        var requestBody = operation.GetProperty("requestBody");
        await Assert.That(requestBody.GetProperty("required").GetBoolean()).IsTrue();
        var bodySchemaReference = requestBody
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        await Assert.That(bodySchemaReference)
            .IsEqualTo("#/components/schemas/ChangeCurrentUserLocaleRequestBody");

        var publicUsersOperation = json.RootElement
            .GetProperty("paths")
            .GetProperty("/api/users")
            .GetProperty("get");
        await Assert.That(publicUsersOperation.GetProperty("security").GetArrayLength()).IsEqualTo(0);
        await Assert.That(publicUsersOperation.GetProperty("responses").TryGetProperty("413", out _)).IsFalse();
        var badRequestContent = publicUsersOperation
            .GetProperty("responses")
            .GetProperty("400")
            .GetProperty("content");
        await Assert.That(badRequestContent.TryGetProperty("application/json", out _)).IsFalse();
        await Assert.That(badRequestContent.TryGetProperty("application/problem+json", out _)).IsTrue();

        foreach (var errorType in new[] { nameof(LocaleRequiredError), nameof(UnsupportedLocaleError) })
        {
            var discriminator = json.RootElement
                .GetProperty("components")
                .GetProperty("schemas")
                .GetProperty(errorType)
                .GetProperty("properties")
                .GetProperty("$type");
            await Assert.That(discriminator.GetProperty("const").GetString()).IsEqualTo(errorType);
        }

        var tags = json.RootElement.GetProperty("tags").EnumerateArray().ToArray();
        await Assert.That(tags.All(tag =>
                tag.TryGetProperty("description", out var description) &&
                !string.IsNullOrWhiteSpace(description.GetString())))
            .IsTrue();
    }

    [Test]
    public async Task LocalizationCatalogs_HaveExactParityAndCoverEveryStableKey()
    {
        using var scope = Fixture.Services.CreateScope();
        var localizer = scope.ServiceProvider.GetRequiredService<IStringLocalizer<ApiResources>>();
        var englishKeys = GetResourceKeys(localizer, Locale.EnglishCode);
        var russianKeys = GetResourceKeys(localizer, Locale.RussianCode);

        var expectedKeys = typeof(ValidationErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .Concat([
                "invalid_request_title",
                "payload_too_large_title",
                "unexpected_problem_title",
                "validation_problem_title"
            ])
            .ToHashSet(StringComparer.Ordinal);

        await Assert.That(englishKeys).IsEquivalentTo(expectedKeys);
        await Assert.That(russianKeys).IsEquivalentTo(expectedKeys);
    }

    private static HashSet<string> GetResourceKeys(
        IStringLocalizer<ApiResources> localizer,
        string locale)
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
            return localizer.GetAllStrings(includeParentCultures: false)
                .Select(resource => resource.Name)
                .ToHashSet(StringComparer.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }
}
