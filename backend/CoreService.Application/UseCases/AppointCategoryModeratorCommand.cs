using System.Data;
using CoreService.Application.Authorization;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.Domain.Errors;

namespace CoreService.Application.UseCases;

using CommandResult = SuccessOr<
    PermissionDeniedError,
    CategoryNotFoundError,
    UserNotFoundError,
    InvalidCategoryModeratorValidityError,
    DuplicateCategoryModeratorAppointmentError>;

public sealed class AppointCategoryModeratorCommand : ICommand<CommandResult>
{
    public required CategoryId CategoryId { get; init; }
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime AppointedAt { get; init; }
    public DateTime? ValidUntil { get; init; }
}

public sealed class AppointCategoryModeratorCommandHandler(
    ICategoryWriteRepository categories,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<AppointCategoryModeratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        AppointCategoryModeratorCommand command,
        CancellationToken cancellationToken)
    {
        var validUntil = command.ValidUntil is { } requestedValidUntil
            ? requestedValidUntil.ToUniversalTime()
            : (DateTime?)null;
        if (validUntil <= command.AppointedAt)
            return new InvalidCategoryModeratorValidityError();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var categoryResult = await categories.GetAsync(command.CategoryId, cancellationToken);
        if (!categoryResult.TryGetValue(out var category, out var categoryError)) return categoryError;

        var scope = AuthorizationScope.Category(category.ForumId, category.CategoryId);
        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            command.AppointedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        if (!await users.IsActiveAsync(command.UserId, cancellationToken)) return new UserNotFoundError();

        var existing = await grants.GetUnrevokedCategoryModeratorGrantsAsync(
            command.UserId,
            command.CategoryId,
            cancellationToken);
        if (existing.Any(grant => grant.IsActiveAt(command.AppointedAt)))
            return new DuplicateCategoryModeratorAppointmentError(command.UserId, command.CategoryId);

        foreach (var expiredGrant in existing)
            expiredGrant.Revoke(command.RequestedBy.UserId, command.AppointedAt);

        var assignmentId = AuthorizationAssignmentId.From(Guid.CreateVersion7());
        grants.AddRange(CategoryModeratorCapabilities.All.Select(capability => CapabilityGrant.Create(
            assignmentId,
            command.UserId,
            capability,
            scope,
            GrantSourceType.CategoryModeratorAppointment,
            command.RequestedBy.UserId,
            command.AppointedAt,
            validUntil)));

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
