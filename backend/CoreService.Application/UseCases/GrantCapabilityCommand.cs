using System.Data;
using CoreService.Domain.Authorization;
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
    CapabilityGrantId,
    PermissionDeniedError,
    UserNotFoundError,
    AuthorizationScopeNotFoundError,
    InvalidAuthorizationScopeError,
    CapabilityNotApplicableToScopeError,
    InvalidCapabilityGrantValidityError,
    DuplicateCapabilityGrantError>;

public sealed class GrantCapabilityCommand : ICommand<CommandResult>
{
    public required UserId UserId { get; init; }
    public required CapabilityCode Capability { get; init; }
    public required AuthorizationScopeType ScopeType { get; init; }
    public ForumId? ForumId { get; init; }
    public CategoryId? CategoryId { get; init; }
    public ThreadId? ThreadId { get; init; }
    public DateTime? ValidUntil { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime GrantedAt { get; init; }
}

public sealed class GrantCapabilityCommandHandler(
    IAuthorizationScopeResolver scopes,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies,
    IUserStatusReader users,
    IUnitOfWork unitOfWork) : ICommandHandler<GrantCapabilityCommand, CommandResult>
{
    public async Task<CommandResult> HandleAsync(
        GrantCapabilityCommand command,
        CancellationToken cancellationToken)
    {
        var validUntil = command.ValidUntil is { } requestedValidUntil
            ? requestedValidUntil.ToUniversalTime()
            : (DateTime?)null;
        if (validUntil <= command.GrantedAt) return new InvalidCapabilityGrantValidityError();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var scopeResult = await scopes.ResolveAsync(
            command.ScopeType,
            command.ForumId,
            command.CategoryId,
            command.ThreadId,
            cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var scopeError)) return scopeError;

        if (!CapabilityScopePolicy.IsAllowed(command.Capability, scope.Type))
            return new CapabilityNotApplicableToScopeError(command.Capability, scope.Type);

        var managementAuthorization = await policies.AuthorizeAsync(
            command.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            command.GrantedAt,
            cancellationToken);
        if (managementAuthorization.TryGetFailure(out var managementFailure)) return managementFailure;

        var canDelegateCapability = await grants.HasActiveCapabilityAsync(
            command.RequestedBy.UserId,
            command.Capability,
            scope,
            command.GrantedAt,
            cancellationToken);
        if (!canDelegateCapability)
        {
            // A platform authorization manager owns the permission catalog and can introduce
            // newly deployed capabilities without requiring an out-of-band database change.
            canDelegateCapability = await grants.HasActiveCapabilityAsync(
                command.RequestedBy.UserId,
                CapabilityCode.ManageAuthorization,
                AuthorizationScope.Platform,
                command.GrantedAt,
                cancellationToken);
        }
        if (!canDelegateCapability) return new PermissionDeniedError();

        if (!await users.IsActiveAsync(command.UserId, cancellationToken)) return new UserNotFoundError();

        var existing = await grants.GetUnrevokedDirectGrantAsync(
            command.UserId,
            command.Capability,
            scope,
            cancellationToken);
        if (existing?.IsActiveAt(command.GrantedAt) == true)
            return new DuplicateCapabilityGrantError(command.UserId, command.Capability);
        if (existing is not null) existing.Revoke(command.RequestedBy.UserId, command.GrantedAt);

        var grant = CapabilityGrant.Create(
            AuthorizationAssignmentId.From(Guid.CreateVersion7()),
            command.UserId,
            command.Capability,
            scope,
            GrantSourceType.Direct,
            command.RequestedBy.UserId,
            command.GrantedAt,
            validUntil);
        grants.AddRange([grant]);

        await unitOfWork.CommitAsync(cancellationToken);
        return grant.CapabilityGrantId;
    }
}
