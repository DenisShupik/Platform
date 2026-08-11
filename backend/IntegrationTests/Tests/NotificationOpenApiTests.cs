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
        using var response = await client.GetAsync("api/openapi.json", cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
