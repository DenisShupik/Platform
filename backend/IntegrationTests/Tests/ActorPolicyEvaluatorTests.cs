using CoreService.Application.Authorization;
using NotificationService.Application.Authorization;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class ActorPolicyEvaluatorTests
{
    [Test]
    public async Task ThreadSubscriptionPolicy_AllowsOnlyOwner()
    {
        var ownerId = UserId.From(Guid.NewGuid());
        var otherUserId = UserId.From(Guid.NewGuid());
        var policy = new ThreadSubscriptionPolicyEvaluator();

        var ownerDecision = policy.Authorize(
            new ActorContext(ownerId),
            ThreadSubscriptionPolicy.Manage,
            ownerId);
        var otherUserDecision = policy.Authorize(
            new ActorContext(otherUserId),
            ThreadSubscriptionPolicy.Manage,
            ownerId);

        await Assert.That(ownerDecision.IsSuccess).IsTrue();
        await Assert.That(otherUserDecision.TryGetFailure(out _)).IsTrue();
    }

    [Test]
    public async Task BookmarkPolicy_AllowsOnlyOwner()
    {
        var ownerId = UserId.From(Guid.NewGuid());
        var otherUserId = UserId.From(Guid.NewGuid());
        var policy = new BookmarkPolicyEvaluator();

        var ownerDecision = policy.Authorize(
            new ActorContext(ownerId),
            BookmarkPolicy.Read,
            ownerId);
        var otherUserDecision = policy.Authorize(
            new ActorContext(otherUserId),
            BookmarkPolicy.Read,
            ownerId);

        await Assert.That(ownerDecision.IsSuccess).IsTrue();
        await Assert.That(otherUserDecision.TryGetFailure(out _)).IsTrue();
    }
}
