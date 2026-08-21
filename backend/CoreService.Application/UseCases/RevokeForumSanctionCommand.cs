using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<PermissionDeniedError, ForumSanctionNotFoundError>;

public sealed class RevokeForumSanctionCommand : ICommand<CommandResult>
{
    public required ForumSanctionId ForumSanctionId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RevokedAt { get; init; }
}

public sealed class RevokeForumSanctionCommandHandler(
    IForumSanctionRepository sanctions,
    IForumPolicyEvaluator policies,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokeForumSanctionCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        RevokeForumSanctionCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var sanction = await sanctions.GetUnrevokedAsync(command.ForumSanctionId, cancellationToken);
        if (sanction is null || !sanction.IsActiveAt(command.RevokedAt))
            return new ForumSanctionNotFoundError(command.ForumSanctionId);

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageSanctions,
            sanction.GetScope(),
            command.RevokedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        sanction.Revoke(command.RequestedBy.UserId, command.RevokedAt);
        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
