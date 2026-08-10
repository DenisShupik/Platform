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

    [Test]
    public async Task UpdatePost_AdvancesRowVersionAndRejectsStaleVersion(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var postId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var originalPost = await userClient.GetPostAsync(postId, cancellationToken);

        await userClient.UpdatePostAsync(
            postId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("First update"),
                RowVersion = originalPost.RowVersion
            },
            cancellationToken);

        var updatedPost = await userClient.GetPostAsync(postId, cancellationToken);
        await Assert.That(updatedPost.RowVersion).IsNotEqualTo(originalPost.RowVersion);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => userClient.UpdatePostAsync(
            postId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("Stale update"),
                RowVersion = originalPost.RowVersion
            },
            cancellationToken));

        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task UpdatePost_AllowsOnlyAuthorOrModeratorAfterApproval(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var headerPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);
        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        var userPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var userPost = await userClient.GetPostAsync(userPostId, cancellationToken);
        await userClient.UpdatePostAsync(
            userPostId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("Сообщение от автора"),
                RowVersion = userPost.RowVersion
            },
            cancellationToken);

        var moderatorPostId = await moderatorClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var moderatorPost = await userClient.GetPostAsync(moderatorPostId, cancellationToken);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(() => userClient.UpdatePostAsync(
            moderatorPostId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("Чужое сообщение"),
                RowVersion = moderatorPost.RowVersion
            },
            cancellationToken));

        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var ownHeaderPost = await userClient.GetPostAsync(headerPostId, cancellationToken);
        var ownHeaderUpdate = await Assert.ThrowsAsync<HttpRequestException>(() => userClient.UpdatePostAsync(
            headerPostId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("Заголовок от автора"),
                RowVersion = ownHeaderPost.RowVersion
            },
            cancellationToken));

        await Assert.That(ownHeaderUpdate?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var headerPost = await moderatorClient.GetPostAsync(headerPostId, cancellationToken);
        await moderatorClient.UpdatePostAsync(
            headerPostId,
            new UpdatePostRequestBody
            {
                Content = PostContent.From("Отредактировано модератором"),
                RowVersion = headerPost.RowVersion
            },
            cancellationToken);
    }

    [Test]
    public async Task DeletePost_ProtectsApprovedHeadersAndOtherAuthorsPosts(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        var headerPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);
        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        var headerDeletion = await Assert.ThrowsAsync<HttpRequestException>(
            () => userClient.DeletePostAsync(headerPostId, cancellationToken));
        await Assert.That(headerDeletion?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var moderatorHeaderDeletion = await Assert.ThrowsAsync<HttpRequestException>(
            () => moderatorClient.DeletePostAsync(headerPostId, cancellationToken));
        await Assert.That(moderatorHeaderDeletion?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var moderatorPostId = await moderatorClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var foreignPostDeletion = await Assert.ThrowsAsync<HttpRequestException>(
            () => userClient.DeletePostAsync(moderatorPostId, cancellationToken));
        await Assert.That(foreignPostDeletion?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var userPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        await userClient.DeletePostAsync(userPostId, cancellationToken);

        var anotherUserPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        await moderatorClient.DeletePostAsync(anotherUserPostId, cancellationToken);
    }
}
