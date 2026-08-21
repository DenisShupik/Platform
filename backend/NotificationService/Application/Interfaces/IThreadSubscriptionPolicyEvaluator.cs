using NotificationService.Application.Authorization;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionPolicyEvaluator
{
    SuccessOr<PermissionDeniedError> Authorize(
        ActorContext actor,
        ThreadSubscriptionPolicy policy,
        UserId subscriptionOwnerId);
}
