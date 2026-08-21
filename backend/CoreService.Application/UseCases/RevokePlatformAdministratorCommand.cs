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
    PlatformAdministratorAppointmentNotFoundError,
    LastPlatformAdministratorError>;

public sealed class RevokePlatformAdministratorCommand : ICommand<CommandResult>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RevokedAt { get; init; }
}

public sealed class RevokePlatformAdministratorCommandHandler(
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokePlatformAdministratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        RevokePlatformAdministratorCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            AuthorizationScope.Platform,
            command.RevokedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var activeAppointments = await grants.GetActivePlatformAdministratorAppointmentsAsync(
            command.RevokedAt,
            cancellationToken);
        if (activeAppointments.All(appointment => appointment.UserId != command.UserId))
            return new PlatformAdministratorAppointmentNotFoundError(command.UserId);
        var remainingAdministratorIds = activeAppointments
            .Select(appointment => appointment.UserId)
            .Distinct()
            .Where(userId => userId != command.UserId)
            .ToArray();
        var hasReachableAdministrator = false;
        foreach (var userId in remainingAdministratorIds)
        {
            if (!await users.IsActiveAsync(userId, cancellationToken)) continue;
            hasReachableAdministrator = true;
            break;
        }

        if (!hasReachableAdministrator)
            return new LastPlatformAdministratorError();

        var existing = await grants.GetUnrevokedPlatformAdministratorGrantsAsync(
            command.UserId,
            cancellationToken);
        foreach (var grant in existing.Where(grant => grant.IsActiveAt(command.RevokedAt)))
            grant.Revoke(command.RequestedBy.UserId, command.RevokedAt);

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
