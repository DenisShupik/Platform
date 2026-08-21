using System.Net;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Shared.Domain.ValueObjects;
using Shared.Tests.Services;

namespace IntegrationTests.Tests;

public sealed class DirectCapabilityGrantTests
{
    [ClassDataSource<CoreServiceTestsFixture<DirectCapabilityGrantTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<DirectCapabilityGrantTests> Fixture { get; init; }

    [Test]
    public async Task IndividualCapability_IsScopedAndRevocable(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await CreatePendingThreadAsync(administrator, categoryId, cancellationToken);

        using (var forbidden = await user.PostApproveThreadAsync(threadId, cancellationToken))
            await Assert.That(forbidden.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var grantId = await administrator.GrantCapabilityAsync(
            new GrantCapabilityRequestBody
            {
                UserId = Fixture.TestUserId,
                Capability = CapabilityCode.ApproveThreads,
                ScopeType = AuthorizationScopeType.Category,
                ForumId = null,
                CategoryId = categoryId,
                ThreadId = null,
                ValidUntil = null
            },
            cancellationToken);

        var grants = await administrator.GetCapabilityGrantsAsync(
            AuthorizationScopeType.Category,
            null,
            categoryId,
            null,
            cancellationToken);
        await Assert.That(grants).HasSingleItem();
        await Assert.That(grants[0].Capability).IsEqualTo(CapabilityCode.ApproveThreads);

        await user.ApproveThreadAsync(threadId, cancellationToken);
        await administrator.RevokeCapabilityAsync(grantId, cancellationToken);

        var secondThreadId = await CreatePendingThreadAsync(administrator, categoryId, cancellationToken);
        using var forbiddenAfterRevocation = await user.PostApproveThreadAsync(secondThreadId, cancellationToken);
        await Assert.That(forbiddenAfterRevocation.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ThreadCapability_IsMostSpecific_AndScopeMatrixIsEnforced(
        CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var firstThreadId = await CreatePendingThreadAsync(administrator, categoryId, cancellationToken);
        var secondThreadId = await CreatePendingThreadAsync(administrator, categoryId, cancellationToken);

        await administrator.GrantCapabilityAsync(
            ThreadGrant(Fixture.TestUserId, CapabilityCode.ApproveThreads, firstThreadId),
            cancellationToken);

        await user.ApproveThreadAsync(firstThreadId, cancellationToken);
        using (var response = await user.PostApproveThreadAsync(secondThreadId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        using var invalidScope = await administrator.PostGrantCapabilityAsync(
            new GrantCapabilityRequestBody
            {
                UserId = Fixture.TestUserId,
                Capability = CapabilityCode.ManageStructure,
                ScopeType = AuthorizationScopeType.Category,
                ForumId = null,
                CategoryId = categoryId,
                ThreadId = null,
                ValidUntil = null
            },
            cancellationToken);
        await Assert.That(invalidScope.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PlatformAuthorizationManager_CanIntroduceAnotherCapability(
        CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var authorizationManager = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        await administrator.GrantCapabilityAsync(
            new GrantCapabilityRequestBody
            {
                UserId = Fixture.TestUserId,
                Capability = CapabilityCode.ManageAuthorization,
                ScopeType = AuthorizationScopeType.Platform,
                ForumId = null,
                CategoryId = null,
                ThreadId = null,
                ValidUntil = null
            },
            cancellationToken);

        using var response = await authorizationManager.PostGrantCapabilityAsync(
            new GrantCapabilityRequestBody
            {
                UserId = Fixture.TestUserId,
                Capability = CapabilityCode.ManageSanctions,
                ScopeType = AuthorizationScopeType.Platform,
                ForumId = null,
                CategoryId = null,
                ThreadId = null,
                ValidUntil = null
            },
            cancellationToken);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    private static GrantCapabilityRequestBody ThreadGrant(
        UserId userId,
        CapabilityCode capability,
        ThreadId threadId) =>
        new()
        {
            UserId = userId,
            Capability = capability,
            ScopeType = AuthorizationScopeType.Thread,
            ForumId = null,
            CategoryId = null,
            ThreadId = threadId,
            ValidUntil = null
        };

    private static async Task<ThreadId> CreatePendingThreadAsync(
        CoreServiceClient owner,
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var threadId = await owner.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await owner.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await owner.RequestThreadApprovalAsync(threadId, cancellationToken);
        return threadId;
    }
}
