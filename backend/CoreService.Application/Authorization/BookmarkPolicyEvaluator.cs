using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Authorization;

public sealed class BookmarkPolicyEvaluator : IBookmarkPolicyEvaluator
{
    public SuccessOr<PermissionDeniedError> Authorize(
        ActorContext actor,
        BookmarkPolicy policy,
        UserId bookmarkOwnerId)
    {
        if (policy is not BookmarkPolicy.Read)
            throw new ArgumentOutOfRangeException(nameof(policy), policy, null);

        return actor.UserId == bookmarkOwnerId
            ? SuccessOr.Success
            : new PermissionDeniedError();
    }
}
