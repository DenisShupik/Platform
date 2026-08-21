using CoreService.Application.Authorization;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IForumPolicyEvaluator
{
    Task<IReadOnlySet<ForumPolicy>> GetAllowedAsync(
        ActorContext actor,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<SuccessOr<PermissionDeniedError>> AuthorizeAsync(
        ActorContext actor,
        ForumPolicy policy,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);
}
