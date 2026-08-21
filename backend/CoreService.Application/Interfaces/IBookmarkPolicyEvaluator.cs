using CoreService.Application.Authorization;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IBookmarkPolicyEvaluator
{
    SuccessOr<PermissionDeniedError> Authorize(
        ActorContext actor,
        BookmarkPolicy policy,
        UserId bookmarkOwnerId);
}
