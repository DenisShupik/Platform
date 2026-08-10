using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Localization;
using HeaderNames = Microsoft.Net.Http.Headers.HeaderNames;

namespace IntegrationTests.Tests;

public sealed class BackendLocalizationTests
{
    [ClassDataSource<CoreServiceTestsFixture<BackendLocalizationTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<BackendLocalizationTests> Fixture { get; init; }

    [Test]
    public async Task ValidationProblem_UsesRequestedLocaleAndStableCode(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));

        using var response = await client.GetAsync("api/forums?limit=invalid", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
        await Assert.That(response.Content.Headers.ContentType?.MediaType)
            .IsEqualTo("application/problem+json");
        await Assert.That(response.Content.Headers.ContentLanguage).Contains(Locale.RussianCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        await Assert.That(json.RootElement.GetProperty("code").GetString())
            .IsEqualTo("validation_failed");
        await Assert.That(json.RootElement.GetProperty("traceId").GetString()).IsNotEmpty();
        var error = json.RootElement.GetProperty("errors").GetProperty("limit");
        await Assert.That(error.GetProperty("code").GetString())
            .IsEqualTo("cannot_parse_input_value");
        await Assert.That(error.GetProperty("message").GetString()).Contains("формат");
    }

    [Test]
    public async Task TitleFiltering_IsCaseInsensitiveAndIndependentOfRequestCulture(
        CancellationToken cancellationToken)
    {
        var moderator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var forumId = await moderator.CreateForumAsync(new()
        {
            Title = ForumTitle.From("Culture Search Forum")
        }, cancellationToken);

        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));
        using var response = await client.GetAsync(
            "api/forums?title=CULTURE%20SEARCH",
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        await Assert.That(responseBody).Contains(forumId.Value.ToString());
    }

    [Test]
    public async Task ResponseLocalization_NormalizesHeadersCopiedFromDownstream()
    {
        var context = new DefaultHttpContext();
        context.Response.Headers[HeaderNames.ContentLanguage] =
            new StringValues([Locale.RussianCode, Locale.RussianCode]);
        context.Response.Headers[HeaderNames.Vary] =
            new StringValues(["Origin", HeaderNames.AcceptLanguage, HeaderNames.AcceptLanguage]);

        ApiResponseLocalization.Apply(context.Response, Locale.RussianCode);

        var contentLanguages = context.Response.Headers[HeaderNames.ContentLanguage]
            .Select(value => value!)
            .ToArray();
        await Assert.That(contentLanguages)
            .IsEquivalentTo([Locale.RussianCode]);
        await Assert.That(context.Response.Headers[HeaderNames.Vary].ToString())
            .IsEqualTo($"Origin, {HeaderNames.AcceptLanguage}");
    }

    [Test]
    public async Task OpenApi_KeepsDomainDiscriminatorInsideProblemDetailsUnion(
        CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        var content = json.RootElement
            .GetProperty("paths")
            .GetProperty("/api/search")
            .GetProperty("get")
            .GetProperty("responses")
            .GetProperty("400")
            .GetProperty("content");
        var domainSchema = content
            .GetProperty("application/json")
            .GetProperty("schema");
        await Assert.That(domainSchema.GetProperty("discriminator")
                .GetProperty("propertyName").GetString())
            .IsEqualTo("$type");

        var problemSchema = content
            .GetProperty("application/problem+json")
            .GetProperty("schema");
        var branches = problemSchema.GetProperty("oneOf").EnumerateArray().ToArray();
        await Assert.That(branches.Any(branch =>
                branch.TryGetProperty("$ref", out var reference) &&
                reference.GetString()!.EndsWith("/ApiProblemDetails", StringComparison.Ordinal)))
            .IsTrue();
        await Assert.That(branches.Any(branch =>
                branch.TryGetProperty("$ref", out var reference) &&
                reference.GetString()!.EndsWith("/ApiValidationProblemDetails", StringComparison.Ordinal)))
            .IsTrue();

        var problemDetails = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty("ApiProblemDetails");
        await Assert.That(problemDetails.GetProperty("properties").TryGetProperty("extensions", out _))
            .IsFalse();
        await Assert.That(problemDetails.GetProperty("required")
                .EnumerateArray()
                .Any(value => value.GetString() == "extensions"))
            .IsFalse();

        var rowVersionSchema = json.RootElement
            .GetProperty("components")
            .GetProperty("schemas")
            .GetProperty(nameof(RowVersion));

        await Assert.That(rowVersionSchema.GetProperty("type").GetString()).IsEqualTo("integer");
        await Assert.That(rowVersionSchema.GetProperty("format").GetString()).IsEqualTo("uint32");
        await Assert.That(rowVersionSchema.GetProperty("minimum").GetUInt32()).IsEqualTo(uint.MinValue);
        await Assert.That(rowVersionSchema.GetProperty("maximum").GetUInt32()).IsEqualTo(uint.MaxValue);
    }
}
