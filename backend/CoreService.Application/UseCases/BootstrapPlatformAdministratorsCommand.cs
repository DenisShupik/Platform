using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class BootstrapPlatformAdministratorsCommand : ICommand<SuccessOr<UserNotFoundError>>
{
    public required IReadOnlyList<UserId> UserIds { get; init; }
    public required DateTime BootstrappedAt { get; init; }
}

/// <summary>
/// Grants the initial platform capability bundle. The operation is exposed only through
/// the internal service-authenticated transport and becomes a no-op after the first active grant.
/// </summary>
public sealed class BootstrapPlatformAdministratorsCommandHandler(
    ICapabilityGrantRepository grants,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<BootstrapPlatformAdministratorsCommand, SuccessOr<UserNotFoundError>>
{
    public async Task<SuccessOr<UserNotFoundError>> HandleAsync(
        BootstrapPlatformAdministratorsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.UserIds.Count == 0) return SuccessOr.Success;

        var requestedUserIds = command.UserIds.Distinct().ToArray();
        foreach (var userId in requestedUserIds)
        {
            if (!await users.IsActiveAsync(userId, cancellationToken)) return new UserNotFoundError();
        }

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var activeAppointments = await grants.GetActivePlatformAdministratorAppointmentsAsync(
            command.BootstrappedAt,
            cancellationToken);
        foreach (var administratorId in activeAppointments.Select(appointment => appointment.UserId).Distinct())
        {
            if (await users.IsActiveAsync(administratorId, cancellationToken)) return SuccessOr.Success;
        }

        foreach (var userId in requestedUserIds)
        {
            var assignmentId = AuthorizationAssignmentId.From(Guid.CreateVersion7());
            grants.AddRange(PlatformAdministratorCapabilities.All.Select(capability => CapabilityGrant.Create(
                assignmentId,
                userId,
                capability,
                AuthorizationScope.Platform,
                GrantSourceType.PlatformAdministratorBootstrap,
                null,
                command.BootstrappedAt)));
        }

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
