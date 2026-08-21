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

using CommandResult = Result<
    ForumSanctionId,
    PermissionDeniedError,
    UserNotFoundError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError,
    InvalidForumSanctionValidityError,
    DuplicateForumSanctionError>;

public sealed class IssueForumSanctionCommand : ICommand<CommandResult>
{
    public required UserId UserId { get; init; }
    public required ForumSanctionType Type { get; init; }
    public required AuthorizationScopeType ScopeType { get; init; }
    public ForumId? ForumId { get; init; }
    public CategoryId? CategoryId { get; init; }
    public ThreadId? ThreadId { get; init; }
    public required ForumSanctionReason Reason { get; init; }
    public DateTime? ValidUntil { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime IssuedAt { get; init; }
}

public sealed class IssueForumSanctionCommandHandler(
    IAuthorizationScopeResolver scopes,
    IForumSanctionRepository sanctions,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<IssueForumSanctionCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        IssueForumSanctionCommand command,
        CancellationToken cancellationToken)
    {
        var validUntil = command.ValidUntil is { } requestedValidUntil
            ? requestedValidUntil.ToUniversalTime()
            : (DateTime?)null;
        if (validUntil <= command.IssuedAt) return new InvalidForumSanctionValidityError();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var scopeResult = await scopes.ResolveAsync(
            command.ScopeType,
            command.ForumId,
            command.CategoryId,
            command.ThreadId,
            cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var scopeError)) return scopeError;

        var authorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageSanctions,
            scope,
            command.IssuedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        if (!await users.ExistsAsync(command.UserId, cancellationToken)) return new UserNotFoundError();
        if (command.UserId == command.RequestedBy.UserId) return new PermissionDeniedError();

        var administratorGrants = await grants.GetUnrevokedPlatformAdministratorGrantsAsync(
            command.UserId,
            cancellationToken);
        if (administratorGrants.Any(grant => grant.IsActiveAt(command.IssuedAt)))
            return new PermissionDeniedError();

        var existing = await sanctions.GetUnrevokedAsync(
            command.UserId,
            command.Type,
            scope,
            cancellationToken);
        if (existing?.IsActiveAt(command.IssuedAt) == true)
            return new DuplicateForumSanctionError(command.UserId, command.Type);
        if (existing is not null) existing.Revoke(command.RequestedBy.UserId, command.IssuedAt);

        var sanction = ForumSanction.Issue(
            command.UserId,
            command.Type,
            scope,
            command.Reason,
            command.RequestedBy.UserId,
            command.IssuedAt,
            validUntil);
        sanctions.Add(sanction);

        await unitOfWork.CommitAsync(cancellationToken);
        return sanction.ForumSanctionId;
    }
}
