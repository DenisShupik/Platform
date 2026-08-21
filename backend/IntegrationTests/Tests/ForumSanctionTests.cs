using System.Net;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;

namespace IntegrationTests.Tests;

public sealed class ForumSanctionTests
{
    [ClassDataSource<CoreServiceTestsFixture<ForumSanctionTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<ForumSanctionTests> Fixture { get; init; }

    [Test]
    public async Task NoAccessAndReadOnly_AreEnforcedAndRevocable(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var anonymous = Fixture.GetCoreServiceClient();
        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await CreateApprovedThreadAsync(administrator, categoryId, cancellationToken);

        var noAccessId = await administrator.IssueForumSanctionAsync(
            new IssueForumSanctionRequestBody
            {
                UserId = Fixture.TestUserId,
                Type = ForumSanctionType.NoAccess,
                ScopeType = AuthorizationScopeType.Thread,
                ForumId = null,
                CategoryId = null,
                ThreadId = threadId,
                Reason = ForumSanctionReason.From("Repeated abuse"),
                ValidUntil = DateTime.UtcNow.AddHours(1)
            },
            cancellationToken);

        using (var hidden = await user.GetThreadResponseAsync(threadId, cancellationToken))
            await Assert.That(hidden.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
        using (var publicThread = await anonymous.GetThreadResponseAsync(threadId, cancellationToken))
            await Assert.That(publicThread.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using (var blockedPost = await user.PostCreatePostAsync(
                   threadId,
                   TestRequests.CreateHeaderPost,
                   cancellationToken))
            await Assert.That(blockedPost.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        await administrator.RevokeForumSanctionAsync(noAccessId, cancellationToken);
        using (var visibleAgain = await user.GetThreadResponseAsync(threadId, cancellationToken))
            await Assert.That(visibleAgain.StatusCode).IsEqualTo(HttpStatusCode.OK);

        await administrator.IssueForumSanctionAsync(
            new IssueForumSanctionRequestBody
            {
                UserId = Fixture.TestUserId,
                Type = ForumSanctionType.ReadOnly,
                ScopeType = AuthorizationScopeType.Forum,
                ForumId = forumId,
                CategoryId = null,
                ThreadId = null,
                Reason = ForumSanctionReason.From("Temporary read-only mode"),
                ValidUntil = DateTime.UtcNow.AddHours(1)
            },
            cancellationToken);

        using (var readable = await user.GetThreadResponseAsync(threadId, cancellationToken))
            await Assert.That(readable.StatusCode).IsEqualTo(HttpStatusCode.OK);
        using var readOnlyPost = await user.PostCreatePostAsync(
            threadId,
            TestRequests.CreateHeaderPost,
            cancellationToken);
        await Assert.That(readOnlyPost.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task NoAccess_HidesForumAndCategoryMetadata(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);

        await administrator.IssueForumSanctionAsync(
            new IssueForumSanctionRequestBody
            {
                UserId = Fixture.TestUserId,
                Type = ForumSanctionType.NoAccess,
                ScopeType = AuthorizationScopeType.Forum,
                ForumId = forumId,
                CategoryId = null,
                ThreadId = null,
                Reason = ForumSanctionReason.From("Metadata must be hidden"),
                ValidUntil = DateTime.UtcNow.AddHours(1)
            },
            cancellationToken);

        using (var forumResponse = await user.GetForumResponseAsync(forumId, cancellationToken))
            await Assert.That(forumResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
        using (var categoryResponse = await user.GetCategoryResponseAsync(categoryId, cancellationToken))
            await Assert.That(categoryResponse.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SelfAndPlatformAdministratorSanctions_AreRejected(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);

        using (var selfSanction = await administrator.PostIssueForumSanctionAsync(
                   PlatformNoAccess(Fixture.TestModeratorUserId),
                   cancellationToken))
            await Assert.That(selfSanction.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var protectedAdministratorId = Shared.Domain.ValueObjects.UserId.From(Guid.NewGuid());
        Fixture.UserStatusReader.SetActive(protectedAdministratorId, true);
        await administrator.AppointPlatformAdministratorAsync(protectedAdministratorId, cancellationToken);

        using var administratorSanction = await administrator.PostIssueForumSanctionAsync(
            PlatformNoAccess(protectedAdministratorId),
            cancellationToken);
        await Assert.That(administratorSanction.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        await administrator.RevokePlatformAdministratorAsync(protectedAdministratorId, cancellationToken);
        Fixture.UserStatusReader.Remove(protectedAdministratorId);
    }

    private static async Task<CoreService.Domain.ValueObjects.ThreadId> CreateApprovedThreadAsync(
        Shared.Tests.Services.CoreServiceClient administrator,
        CoreService.Domain.ValueObjects.CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var threadId = await administrator.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await administrator.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await administrator.RequestThreadApprovalAsync(threadId, cancellationToken);
        await administrator.ApproveThreadAsync(threadId, cancellationToken);
        return threadId;
    }

    private static IssueForumSanctionRequestBody PlatformNoAccess(Shared.Domain.ValueObjects.UserId userId) =>
        new()
        {
            UserId = userId,
            Type = ForumSanctionType.NoAccess,
            ScopeType = AuthorizationScopeType.Platform,
            ForumId = null,
            CategoryId = null,
            ThreadId = null,
            Reason = ForumSanctionReason.From("Administrative safety test"),
            ValidUntil = null
        };
}
