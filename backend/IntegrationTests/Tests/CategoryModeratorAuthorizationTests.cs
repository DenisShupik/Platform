using System.Net;
using CoreService.Domain.Enums;
using CoreService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class CategoryModeratorAuthorizationTests
{
    [ClassDataSource<CoreServiceTestsFixture<CategoryModeratorAuthorizationTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<CategoryModeratorAuthorizationTests> Fixture { get; init; }

    [Test]
    public async Task Appointment_IsScopedAndRevocationTakesEffect(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var user = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var firstCategoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);
        var secondCategoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);

        var firstThreadId = await CreatePendingThreadAsync(user, firstCategoryId, cancellationToken);
        var secondThreadId = await CreatePendingThreadAsync(user, secondCategoryId, cancellationToken);

        using (var response = await user.PostApproveThreadAsync(firstThreadId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        var actionsBeforeAppointment = await user.GetCategoryAllowedActionsAsync(
            firstCategoryId,
            cancellationToken);
        await Assert.That(actionsBeforeAppointment.CanApproveThread).IsFalse();
        await Assert.That(actionsBeforeAppointment.CanManageModerators).IsFalse();

        using (var response = await user.GetCategoryModeratorsResponseAsync(firstCategoryId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        await administrator.AppointCategoryModeratorAsync(
            firstCategoryId,
            Fixture.TestUserId,
            cancellationToken);

        var actionsAfterAppointment = await user.GetCategoryAllowedActionsAsync(
            firstCategoryId,
            cancellationToken);
        await Assert.That(actionsAfterAppointment.CanViewUnpublishedThreads).IsTrue();
        await Assert.That(actionsAfterAppointment.CanApproveThread).IsTrue();
        await Assert.That(actionsAfterAppointment.CanRejectThread).IsTrue();
        await Assert.That(actionsAfterAppointment.CanEditAnyPost).IsTrue();
        await Assert.That(actionsAfterAppointment.CanDeleteAnyPost).IsTrue();
        await Assert.That(actionsAfterAppointment.CanManageModerators).IsFalse();

        var appointments = await administrator.GetCategoryModeratorsAsync(firstCategoryId, cancellationToken);
        await Assert.That(appointments).HasSingleItem();
        await Assert.That(appointments[0].UserId).IsEqualTo(Fixture.TestUserId);
        await Assert.That(appointments[0].GrantedBy).IsEqualTo(Fixture.TestModeratorUserId);

        await user.ApproveThreadAsync(firstThreadId, cancellationToken);
        using (var response = await user.PostApproveThreadAsync(secondThreadId, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        using (var scope = Fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
            var grants = await dbContext.CapabilityGrants
                .Where(grant => grant.UserId == Fixture.TestUserId && grant.CategoryId == firstCategoryId)
                .ToListAsync(cancellationToken);

            await Assert.That(grants.Select(grant => grant.Capability)).IsEquivalentTo(
            [
                CapabilityCode.ViewUnpublishedThreads,
                CapabilityCode.ApproveThreads,
                CapabilityCode.RejectThreads,
                CapabilityCode.EditAnyPost,
                CapabilityCode.DeleteAnyPost
            ]);
            await Assert.That(grants.Select(grant => grant.AssignmentId).Distinct()).HasSingleItem();
            await Assert.That(grants.All(grant => grant.RevokedAt is null)).IsTrue();
        }

        await administrator.RevokeCategoryModeratorAsync(
            firstCategoryId,
            Fixture.TestUserId,
            cancellationToken);

        var actionsAfterRevocation = await user.GetCategoryAllowedActionsAsync(
            firstCategoryId,
            cancellationToken);
        await Assert.That(actionsAfterRevocation.CanApproveThread).IsFalse();
        await Assert.That(await administrator.GetCategoryModeratorsAsync(firstCategoryId, cancellationToken))
            .IsEmpty();

        var threadAfterRevocation = await CreatePendingThreadAsync(user, firstCategoryId, cancellationToken);
        using (var response = await user.PostApproveThreadAsync(threadAfterRevocation, cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

        using (var scope = Fixture.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
            var grants = await dbContext.CapabilityGrants
                .Where(grant => grant.UserId == Fixture.TestUserId && grant.CategoryId == firstCategoryId)
                .ToListAsync(cancellationToken);
            await Assert.That(grants.All(grant => grant.RevokedAt is not null)).IsTrue();
            await Assert.That(grants.All(grant => grant.RevokedBy == Fixture.TestModeratorUserId)).IsTrue();
        }
    }

    [Test]
    public async Task Appointment_RejectsInvalidValidityAndUnknownUser(CancellationToken cancellationToken)
    {
        var administrator = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var forumId = await administrator.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await administrator.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId), cancellationToken);

        using (var response = await administrator.PostAppointCategoryModeratorAsync(
                   categoryId,
                   Fixture.TestUserId,
                   DateTime.UtcNow.AddMinutes(-1),
                   cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.BadRequest);

        using (var response = await administrator.PostAppointCategoryModeratorAsync(
                   categoryId,
                   UserId.From(Guid.CreateVersion7()),
                   null,
                   cancellationToken))
            await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    private static async Task<CoreService.Domain.ValueObjects.ThreadId> CreatePendingThreadAsync(
        Shared.Tests.Services.CoreServiceClient user,
        CoreService.Domain.ValueObjects.CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        var threadId = await user.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);
        await user.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);
        await user.RequestThreadApprovalAsync(threadId, cancellationToken);
        return threadId;
    }
}
