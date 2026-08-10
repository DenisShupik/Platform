using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Abstractions;
using Shared.Domain.Extensions;
using Shared.Infrastructure.Extensions;

namespace IntegrationTests.Tests;

public sealed class VogenPersistenceTests
{
    [ClassDataSource<CoreServiceTestsFixture<VogenPersistenceTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<VogenPersistenceTests> Fixture { get; init; }

    [Test]
    public async Task ValueObjects_WorkTransparently_InEfCoreAndLinqToDb(CancellationToken cancellationToken)
    {
        var client = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var forumId = await client.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await client.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await client.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var postId = await client.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var post = await client.GetPostAsync(postId, cancellationToken);
        var forumIds = new IdSet<ForumId, Guid>([forumId]);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();

        var efForum = await dbContext.Forums
            .SingleAsync(forum => forumIds.Contains(forum.ForumId), cancellationToken);

        var linqToDbQuery = dbContext.Forums
            .Where(forum => forumIds.Contains(forum.ForumId))
            .Select(forum => new { forum.ForumId, forum.Title });
        var linqToDbCommand = linqToDbQuery.ToLinqToDB().ToSqlQuery();
        var linqToDbForums = await linqToDbQuery
            .ToListAsyncLinqToDB(cancellationToken);

        var titleMatches = await dbContext.Forums
            .Where(forum => forum.Title.Contains(
                ForumTitle.From("тЕсТоВыЙ"),
                StringComparison.OrdinalIgnoreCase))
            .Select(forum => forum.ForumId)
            .ToListAsyncLinqToDB(cancellationToken);

        var tableValueIds = await dbContext
            .ToTvcLinqToDb(forumIds)
            .ToListAsyncLinqToDB(cancellationToken);
        var rowVersionQuery = dbContext.Posts
            .Where(entity => entity.PostId == postId && entity.RowVersion == post.RowVersion)
            .Select(entity => new { entity.PostId, entity.RowVersion });
        var rowVersionCommand = rowVersionQuery.ToLinqToDB().ToSqlQuery();
        const uint ordinaryUnsignedInteger = 42;
        var ordinaryUnsignedIntegerCommand = dbContext.Posts
            .Select(_ => ordinaryUnsignedInteger)
            .Take(1)
            .ToLinqToDB()
            .ToSqlQuery();
        var linqToDbPost = await rowVersionQuery.SingleAsyncLinqToDB(cancellationToken);
        var efPost = await dbContext.Posts.SingleAsync(
            entity => entity.PostId == postId && entity.RowVersion == post.RowVersion,
            cancellationToken);

        await Assert.That(efForum.ForumId).IsEqualTo(forumId);
        await Assert.That(linqToDbForums).HasSingleItem();
        await Assert.That(linqToDbForums[0].ForumId).IsEqualTo(forumId);
        await Assert.That(linqToDbForums[0].Title).IsEqualTo(TestRequests.CreateForum.Title);
        await Assert.That(titleMatches).Contains(forumId);
        await Assert.That(linqToDbCommand.Sql).Contains(" = ANY(");
        await Assert.That(linqToDbCommand.Sql.Contains(forumId.Value.ToString(), StringComparison.OrdinalIgnoreCase))
            .IsFalse();
        await Assert.That(linqToDbCommand.Parameters.Count).IsEqualTo(1);
        await Assert.That(linqToDbCommand.Parameters[0].Value is Guid[]).IsTrue();
        await Assert.That(tableValueIds).IsEquivalentTo(forumIds);
        await Assert.That(rowVersionCommand.Sql).Contains("xmin");
        await Assert.That(rowVersionCommand.Parameters.Single(parameter => parameter.Name == "RowVersion").Value is uint)
            .IsTrue();
        await Assert.That(rowVersionCommand.Parameters.Single(parameter => parameter.Name == "RowVersion").DbType)
            .IsEqualTo("xid");
        await Assert.That(ordinaryUnsignedIntegerCommand.Parameters.Single().DbType).IsNotEqualTo("xid");
        await Assert.That(linqToDbPost.RowVersion).IsEqualTo(post.RowVersion);
        await Assert.That(efPost.RowVersion).IsEqualTo(post.RowVersion);
    }
}
