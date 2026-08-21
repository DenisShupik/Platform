using NotificationService.Application.Interfaces;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Authorization;

public sealed class ThreadSubscriptionPolicyEvaluator : IThreadSubscriptionPolicyEvaluator
{
    public SuccessOr<PermissionDeniedError> Authorize(
        ActorContext actor,
        ThreadSubscriptionPolicy policy,
        UserId subscriptionOwnerId)
    {
        return policy switch
        {
            ThreadSubscriptionPolicy.Read or ThreadSubscriptionPolicy.Manage
                when actor.UserId == subscriptionOwnerId => SuccessOr.Success,
            ThreadSubscriptionPolicy.Read or ThreadSubscriptionPolicy.Manage => new PermissionDeniedError(),
            _ => throw new ArgumentOutOfRangeException(nameof(policy), policy, null)
        };
    }
}
