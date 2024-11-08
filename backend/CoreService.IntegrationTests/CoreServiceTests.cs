using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreService.Application.DTOs;
using CoreService.Infrastructure.Persistence;

namespace CoreService.IntegrationTests;

[Collection(nameof(IntegrationTestFactoryCollection))]
public sealed class CoreServiceTests
{
    private readonly IntegrationTestFactory _factory;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    public CoreServiceTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _dbContextFactory = _factory.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    }

    [Fact]
    public async Task CreateThread_ReturnsOk()
    {
        var client = _factory.CreateClient();
        const string title = "Test Thread";
        var response = await client.PostAsJsonAsync("api/threads", new CreateThreadRequest
        {
            Title = title
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var threadId = await response.Content.ReadFromJsonAsync<long>();
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var thread = await dbContext.Threads
            .Where(e => e.ThreadId == threadId)
            .FirstOrDefaultAsync();
        Assert.NotNull(thread);
        Assert.Equal(title, thread.Title);
    }

    // [Fact]
    // public async Task GetUserNotesCount_ReturnsNotFound()
    // {
    //     var client = _factory.CreateClient();
    //     var userId = Guid.Parse("3032740C-5BCC-4FEB-BC95-7ED4E2C74444");
    //     var response = await client.GetAsync($"api/users/{userId}/notes/count");
    //     Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    // }
}