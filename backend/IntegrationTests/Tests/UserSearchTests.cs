using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Persistence;

namespace IntegrationTests.Tests;

public sealed class UserSearchTests
{
    [ClassDataSource<UserServiceTestsFixture<UserSearchTests>>(Shared = SharedType.PerClass)]
    public required UserServiceTestsFixture<UserSearchTests> Fixture { get; init; }

    [Test]
    public async Task UsernameFilter_IsCaseInsensitiveAndServerSide(CancellationToken cancellationToken)
    {
        var userId = UserId.From(Guid.CreateVersion7());
        const string username = "searchneedleuser";

        await using (var scope = Fixture.Services.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<WriteApplicationDbContext>();
            dbContext.Users.Add(new User(
                userId,
                Username.From(username),
                $"{username}@app.com",
                true,
                DateTime.UtcNow));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        using var client = Fixture.CreateClient();
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en");
        using var response = await client.GetAsync("api/users?username=NEEDLE", cancellationToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await using var content = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
        var usernames = document.RootElement
            .EnumerateArray()
            .Select(user => user.GetProperty("username").GetString())
            .ToList();

        await Assert.That(usernames).Contains(username);
    }
}
