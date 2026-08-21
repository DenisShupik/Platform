using System.Net;

namespace IntegrationTests.Tests;

public sealed class ForumModeratorAuthorizationTests
{
    [ClassDataSource<CoreServiceTestsFixture<ForumModeratorAuthorizationTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<ForumModeratorAuthorizationTests> Fixture { get; init; }

    [Test]
    public async Task Appointment_AppliesToEveryCategoryInForum(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var moderator = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var firstCategoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var secondCategoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var firstThreadId = await CreatePendingThreadAsync(administrator, firstCategoryId, cancellationToken);
        var secondThreadId = await CreatePendingThreadAsync(administrator, secondCategoryId, cancellationToken);

        using (var response = await moderator.PostApproveThreadAsync(firstThreadId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        await administrator.AppointForumModeratorAsync(forumId, Fixture.TestUserId, cancellationToken);

        var appointments = await administrator.GetForumModeratorsAsync(forumId, cancellationToken);
        await Assert.That(appointments).HasSingleItem();
        await Assert.That(appointments[0].UserId).IsEqualTo(Fixture.TestUserId);

        var firstActions = await moderator.GetCategoryAllowedActionsAsync(firstCategoryId, cancellationToken);
        var secondActions = await moderator.GetCategoryAllowedActionsAsync(secondCategoryId, cancellationToken);
        await Assert.That(firstActions.CanApproveThread).IsTrue();
        await Assert.That(secondActions.CanApproveThread).IsTrue();

        await moderator.ApproveThreadAsync(firstThreadId, cancellationToken);
        await moderator.ApproveThreadAsync(secondThreadId, cancellationToken);

        await administrator.RevokeForumModeratorAsync(forumId, Fixture.TestUserId, cancellationToken);
        await Assert.That(await administrator.GetForumModeratorsAsync(forumId, cancellationToken)).IsEmpty();

        var thirdThreadId = await CreatePendingThreadAsync(administrator, firstCategoryId, cancellationToken);
        using var forbidden = await moderator.PostApproveThreadAsync(thirdThreadId, cancellationToken);
        await Assert.That(forbidden.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    private static async Task<CoreService.Domain.ValueObjects.ThreadId> CreatePendingThreadAsync(
        Shared.Tests.Services.CoreServiceClient owner,
        CoreService.Domain.ValueObjects.CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var threadId = await owner.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await owner.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await owner.RequestThreadApprovalAsync(threadId, cancellationToken);
        return threadId;
    }
}
