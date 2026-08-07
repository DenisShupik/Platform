using System.Net;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;

namespace IntegrationTests.Tests;

public sealed class UpdatePostTests
{
    [ClassDataSource<CoreServiceTestsFixture<UpdatePostTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<UpdatePostTests> Fixture { get; init; }

    [Test]
    public async Task UpdatePost_RejectsRawHtml(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var postId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var post = await userClient.GetPostAsync(postId, cancellationToken);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => userClient.UpdatePostAsync(
            postId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("<script>alert(1)</script>"),
                RowVersion = post.RowVersion
            },
            cancellationToken));

        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }
}
