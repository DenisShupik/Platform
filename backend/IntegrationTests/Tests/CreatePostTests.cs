namespace IntegrationTests.Tests;

public sealed class CreatePostTests
{
    [ClassDataSource<CoreServiceTestsFixture<CreatePostTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<CreatePostTests> Fixture { get; init; }

    [Test]
    public async Task ParallelCreatePosts_Success(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);

        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);

        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        var tasks = Enumerable.Range(0, 10).Select(x => userClient.CreatePostAsync(threadId, TestRequests.CreatePost,
            cancellationToken));

        await Task.WhenAll(tasks);
    }
}