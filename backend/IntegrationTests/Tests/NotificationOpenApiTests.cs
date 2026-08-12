using System.Net.Http.Headers;
using System.Text.Json;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class NotificationOpenApiTests
{
    [ClassDataSource<NotificationServiceTestsFixture<NotificationOpenApiTests>>(
        Shared = SharedType.PerClass)]
    public required NotificationServiceTestsFixture<NotificationOpenApiTests> Fixture { get; init; }

    [Test]
    public async Task OpenApiDocument_IsGenerated(CancellationToken cancellationToken)
    {
        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.Add(
            new StringWithQualityHeaderValue(Locale.RussianCode));
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);

        response.EnsureSuccessStatusCode();
        await Assert.That(response.Content.Headers.ContentLanguage)
            .IsEquivalentTo([Locale.EnglishCode]);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStreamAsync(cancellationToken));
        await Assert.That(json.RootElement.GetProperty("openapi").GetString()).IsEqualTo("3.2.0");

        var paths = json.RootElement.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
            foreach (var method in path.Value.EnumerateObject().Where(property =>
                         property.Name is "get" or "post" or "put" or "patch" or "delete"))
                await Assert.That(method.Value.GetProperty("summary").GetString()).IsNotEmpty();

        var schemas = json.RootElement.GetProperty("components").GetProperty("schemas");
        await Assert.That(schemas.GetProperty("InternalNotificationsPagedDto")
                .GetProperty("properties")
                .GetProperty("totalCount")
                .GetProperty("description")
                .GetString())
            .IsEqualTo("Total number of notifications after filtering");
        await Assert.That(schemas.GetProperty("GetThreadSubscriptionStatusQueryResult")
                .GetProperty("properties")
                .GetProperty("isSubscribed")
                .GetProperty("description")
                .GetString())
            .IsEqualTo("Whether the user is subscribed to the thread");
    }
}
