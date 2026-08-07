using System.Net;
using System.Net.Http.Json;
using CoreService.Domain.Errors;

namespace IntegrationTests.Tests;

public sealed class PostBookmarkTests
{
    [ClassDataSource<CoreServiceTestsFixture<PostBookmarkTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<PostBookmarkTests> Fixture { get; init; }

    [Test]
    public async Task ParallelCreateBookmark_ReturnsSuccessAndDuplicateError(
        CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var postId = await userClient.CreatePostAsync(
            threadId, TestRequests.CreateHeaderPost, cancellationToken);

        var responses = await Task.WhenAll(
            userClient.PostBookmarkAsync(postId, cancellationToken),
            userClient.PostBookmarkAsync(postId, cancellationToken));

        try
        {
            await Assert.That(responses.Count(response => response.StatusCode == HttpStatusCode.NoContent))
                .IsEqualTo(1);
            await Assert.That(responses.Count(response => response.StatusCode == HttpStatusCode.Conflict))
                .IsEqualTo(1);

            var conflictResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Conflict);
            var error = await conflictResponse.Content
                .ReadFromJsonAsync<DuplicatePostBookmarkError>(cancellationToken);
            await Assert.That(error)
                .IsEqualTo(new DuplicatePostBookmarkError(Fixture.TestUserId, postId));
        }
        finally
        {
            foreach (var response in responses) response.Dispose();
        }
    }
}
