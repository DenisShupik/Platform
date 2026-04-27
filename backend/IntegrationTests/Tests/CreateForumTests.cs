using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Tests;

public sealed class CreateForumTests
{
    [ClassDataSource<CoreServiceTestsFixture<CreateForumTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<CreateForumTests> Fixture { get; init; }

    [Test]
    public async Task CreateForum_Success(CancellationToken cancellationToken)
    {
        var client = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);

        var forumId = await client.CreateForumAsync(TestRequests.CreateForum, cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var forum = await dbContext.Forums
            .FirstOrDefaultAsync(e => e.ForumId == forumId, cancellationToken);
        await Assert.That(forum).IsNotNull();
        await Assert.That(forum.Title).IsEqualTo(TestRequests.CreateForum.Title);
        await Assert.That(forum.CreatedBy).IsEqualTo(Fixture.TestModeratorUserId);
    }
}