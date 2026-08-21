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
    UserNotFoundError,
    DuplicatePlatformAdministratorAppointmentError>;

public sealed class AppointPlatformAdministratorCommand : ICommand<CommandResult>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime AppointedAt { get; init; }
}

public sealed class AppointPlatformAdministratorCommandHandler(
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<AppointPlatformAdministratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        AppointPlatformAdministratorCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            AuthorizationScope.Platform,
            command.AppointedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        if (!await users.IsActiveAsync(command.UserId, cancellationToken)) return new UserNotFoundError();

        var existing = await grants.GetUnrevokedPlatformAdministratorGrantsAsync(
            command.UserId,
            cancellationToken);
        if (existing.Any(grant => grant.IsActiveAt(command.AppointedAt)))
            return new DuplicatePlatformAdministratorAppointmentError(command.UserId);

        foreach (var expiredGrant in existing)
            expiredGrant.Revoke(command.RequestedBy.UserId, command.AppointedAt);

        var assignmentId = AuthorizationAssignmentId.From(Guid.CreateVersion7());
        grants.AddRange(PlatformAdministratorCapabilities.All.Select(capability => CapabilityGrant.Create(
            assignmentId,
            command.UserId,
            capability,
            AuthorizationScope.Platform,
            GrantSourceType.PlatformAdministratorAppointment,
            command.RequestedBy.UserId,
            command.AppointedAt)));

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
