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
    CategoryNotFoundError,
    CategoryModeratorAppointmentNotFoundError>;

public sealed class RevokeCategoryModeratorCommand : ICommand<CommandResult>
{
    public required CategoryId CategoryId { get; init; }
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime RevokedAt { get; init; }
}

public sealed class RevokeCategoryModeratorCommandHandler(
    ICategoryWriteRepository categories,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokeCategoryModeratorCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        RevokeCategoryModeratorCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var categoryResult = await categories.GetAsync(command.CategoryId, cancellationToken);
        if (!categoryResult.TryGetValue(out var category, out var categoryError)) return categoryError;

        var scope = AuthorizationScope.Category(category.ForumId, category.CategoryId);
        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            command.RevokedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var existing = await grants.GetUnrevokedCategoryModeratorGrantsAsync(
            command.UserId,
            command.CategoryId,
            cancellationToken);
        var activeGrants = existing.Where(grant => grant.IsActiveAt(command.RevokedAt)).ToList();
        if (activeGrants.Count == 0)
            return new CategoryModeratorAppointmentNotFoundError(command.UserId, command.CategoryId);

        foreach (var grant in activeGrants) grant.Revoke(command.RequestedBy.UserId, command.RevokedAt);

        await unitOfWork.CommitAsync(cancellationToken);
        return SuccessOr.Success;
    }
}
