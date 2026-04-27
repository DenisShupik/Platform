using System.Net;
using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace IntegrationTests.Tests;

public sealed class ThreadTests
{
    [ClassDataSource<CoreServiceTestsFixture<ThreadTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<ThreadTests> Fixture { get; init; }

    [Test]
    public async Task ThreadLifecycle_Success(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId =
            await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        
        var threadId =
            await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        
        {
            var thread = await dbContext.Threads.FirstAsync(e => e.ThreadId == threadId, cancellationToken);
            await Assert.That(thread.State).IsEqualTo(ThreadState.Draft);
            await Assert.That(thread.PostCount).IsEqualTo(Count.Default);
            await Assert.That(thread.CreatedBy).IsEqualTo(Fixture.TestUserId);
            await Assert.That(thread.LastHeaderPostId).IsNull();
        }

        var postId = await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        {
            var thread = await dbContext.Threads.FirstAsync(e => e.ThreadId == threadId, cancellationToken);
            await Assert.That(thread.PostCount).IsEqualTo(Count.From(1));
            await Assert.That(thread.LastHeaderPostId).IsEqualTo(postId);
        }

        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);

        {
            var thread = await dbContext.Threads.FirstAsync(e => e.ThreadId == threadId, cancellationToken);
            await Assert.That(thread.State).IsEqualTo(ThreadState.PendingApproval);
        }

        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        {
            var thread = await dbContext.Threads.FirstAsync(e => e.ThreadId == threadId, cancellationToken);
            await Assert.That(thread.State).IsEqualTo(ThreadState.Approved);
        }
    }

    [Test]
    public async Task RequestApproval_Fails_When_NoPosts(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        // Attempt to request approval without any posts
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await userClient.RequestThreadApprovalAsync(threadId, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task RequestApproval_Fails_When_NotOwner(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        // Moderator (not owner) attempts to request approval
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await moderatorClient.RequestThreadApprovalAsync(threadId, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task RequestApproval_Fails_When_AlreadyApproved(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);
        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        // Attempt to request approval again
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await userClient.RequestThreadApprovalAsync(threadId, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task ApproveThread_Fails_When_InDraft(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        // Attempt to approve without requesting approval first (still in Draft)
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await moderatorClient.ApproveThreadAsync(threadId, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddPost_Fails_When_PendingApproval(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);

        // Attempt to add a post when state is PendingApproval
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }

    [Test]
    public async Task AddPost_Fails_When_NotOwner_And_Draft(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        // Moderator (not owner) attempts to post in Draft thread
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await moderatorClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetThreadPosts_Fails_ForAnonymous_When_ThreadNotApproved(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await anonymousClient.GetThreadPostsAsync(threadId, cancellationToken));

        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task AddPost_Fails_When_LimitReached_And_Draft(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        
        for (var i = 0; i < 5; i++)
        {
            await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        }
        
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }
    [Test]
    public async Task RejectThread_Success(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);

        await moderatorClient.RejectThreadAsync(threadId, cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var thread = await dbContext.Threads.FirstAsync(e => e.ThreadId == threadId, cancellationToken);
        await Assert.That(thread.State).IsEqualTo(ThreadState.Draft);
    }

    [Test]
    public async Task RejectThread_Fails_When_InDraft(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        // Attempt to reject without requesting approval first (still in Draft)
        var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await moderatorClient.RejectThreadAsync(threadId, cancellationToken));
        
        await Assert.That(exception?.StatusCode).IsEqualTo(HttpStatusCode.Conflict);
    }
}
