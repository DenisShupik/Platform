using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<PermissionDeniedError, CapabilityGrantNotFoundError>;

public sealed class RevokeCapabilityCommand : ICommand<CommandResult>
{
    public required CapabilityGrantId CapabilityGrantId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RevokedAt { get; init; }
}

public sealed class RevokeCapabilityCommandHandler(
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokeCapabilityCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        RevokeCapabilityCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var grant = await grants.GetUnrevokedDirectGrantAsync(command.CapabilityGrantId, cancellationToken);
        if (grant is null || !grant.IsActiveAt(command.RevokedAt))
            return new CapabilityGrantNotFoundError(command.CapabilityGrantId);

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            grant.GetScope(),
            command.RevokedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        grant.Revoke(command.RequestedBy.UserId, command.RevokedAt);
        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
