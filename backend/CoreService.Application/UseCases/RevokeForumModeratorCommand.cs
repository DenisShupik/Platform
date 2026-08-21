using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    ForumNotFoundError,
    ForumModeratorAppointmentNotFoundError>;

public sealed class RevokeForumModeratorCommand : ICommand<CommandResult>
{
    public required ForumId ForumId { get; init; }
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RevokedAt { get; init; }
}

public sealed class RevokeForumModeratorCommandHandler(
    IForumReadRepository forums,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokeForumModeratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        RevokeForumModeratorCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var scopeResult = await forums.GetAuthorizationScopeAsync(command.ForumId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var forumError)) return forumError;

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            command.RevokedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var existing = await grants.GetUnrevokedForumModeratorGrantsAsync(
            command.UserId,
            command.ForumId,
            cancellationToken);
        var activeGrants = existing.Where(grant => grant.IsActiveAt(command.RevokedAt)).ToList();
        if (activeGrants.Count == 0)
            return new ForumModeratorAppointmentNotFoundError(command.UserId, command.ForumId);

        foreach (var grant in activeGrants) grant.Revoke(command.RequestedBy.UserId, command.RevokedAt);

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
