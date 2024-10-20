using System.Net;

namespace NoteService.integrationTests;

[Collection(nameof(IntegrationTestFactoryCollection))]
public sealed class NoteServiceTests
{
    private readonly IntegrationTestFactory _factory;

    public NoteServiceTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task GetUserNotesCount_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var response = await client.GetAsync($"api/users/{userId}/notes/count");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}