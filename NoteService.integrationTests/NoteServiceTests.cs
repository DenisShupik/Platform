using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NoteService.Domain.Entities;
using NoteService.Infrastructure.Persistence;

namespace NoteService.integrationTests;

[Collection(nameof(IntegrationTestFactoryCollection))]
public sealed class NoteServiceTests
{
    private readonly IntegrationTestFactory _factory;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

    private readonly Note _note;

    public NoteServiceTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _dbContextFactory = _factory.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    }

    [Fact]
    public async Task GetUserNotesCount_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var userId = _factory.Note1.UserId;
        var response = await client.GetAsync($"api/users/{userId}/notes/count");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<int>();
        Assert.Equal(1, content);
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var count = await dbContext.Notes.CountAsync(e => e.UserId == userId);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GetUserNotesCount_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var userId = Guid.Parse("3032740C-5BCC-4FEB-BC95-7ED4E2C74444");
        var response = await client.GetAsync($"api/users/{userId}/notes/count");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}