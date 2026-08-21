using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    ForumNotFoundError,
    UserNotFoundError,
    InvalidForumModeratorValidityError,
    DuplicateForumModeratorAppointmentError>;

public sealed class AppointForumModeratorCommand : ICommand<CommandResult>
{
    public required ForumId ForumId { get; init; }
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime AppointedAt { get; init; }
    public DateTime? ValidUntil { get; init; }
}

public sealed class AppointForumModeratorCommandHandler(
    IForumReadRepository forums,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<AppointForumModeratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        AppointForumModeratorCommand command,
        CancellationToken cancellationToken)
    {
        var validUntil = command.ValidUntil is { } requestedValidUntil
            ? requestedValidUntil.ToUniversalTime()
            : (DateTime?)null;
        if (validUntil <= command.AppointedAt) return new InvalidForumModeratorValidityError();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var scopeResult = await forums.GetAuthorizationScopeAsync(command.ForumId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var forumError)) return forumError;

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            command.AppointedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        if (!await users.IsActiveAsync(command.UserId, cancellationToken)) return new UserNotFoundError();

        var existing = await grants.GetUnrevokedForumModeratorGrantsAsync(
            command.UserId,
            command.ForumId,
            cancellationToken);
        if (existing.Any(grant => grant.IsActiveAt(command.AppointedAt)))
            return new DuplicateForumModeratorAppointmentError(command.UserId, command.ForumId);

        foreach (var expiredGrant in existing)
            expiredGrant.Revoke(command.RequestedBy.UserId, command.AppointedAt);

        var assignmentId = AuthorizationAssignmentId.From(Guid.CreateVersion7());
        grants.AddRange(ForumModeratorCapabilities.All.Select(capability => CapabilityGrant.Create(
            assignmentId,
            command.UserId,
            capability,
            scope,
            GrantSourceType.ForumModeratorAppointment,
            command.RequestedBy.UserId,
            command.AppointedAt,
            validUntil)));

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
