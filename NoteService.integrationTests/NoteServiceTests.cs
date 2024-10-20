using System.Net.Http.Json;
using Microsoft.AspNetCore.Http.HttpResults;

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
    public async Task Get_Information_From_Database_Endpoint()
    {
        var client = _factory.CreateClient();
        var userId = Guid.NewGuid();
        var actual = await client.GetFromJsonAsync<int>($"api/users/{userId}/notes/count");
        Assert.IsType<Results<Ok<int>, NotFound>>(actual);
        Assert.Equal(expected: 10, actual);
    }
}