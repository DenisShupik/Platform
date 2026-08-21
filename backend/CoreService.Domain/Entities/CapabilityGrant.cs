using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Аудируемое назначение одного атомарного полномочия пользователю в заданной области.
/// </summary>
public sealed class CapabilityGrant
{
    public CapabilityGrantId CapabilityGrantId { get; private set; }
    public AuthorizationAssignmentId AssignmentId { get; private set; }
    public UserId UserId { get; private set; }
    public CapabilityCode Capability { get; private set; }
    public AuthorizationScopeType ScopeType { get; private set; }
    public ForumId? ForumId { get; private set; }
    public CategoryId? CategoryId { get; private set; }
    public ThreadId? ThreadId { get; private set; }
    public GrantSourceType SourceType { get; private set; }
    public UserId? GrantedBy { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public UserId? RevokedBy { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private CapabilityGrant()
    {
    }

    public static CapabilityGrant Create(
        AuthorizationAssignmentId assignmentId,
        UserId userId,
        CapabilityCode capability,
        AuthorizationScope scope,
        GrantSourceType sourceType,
        UserId? grantedBy,
        DateTime grantedAt,
        DateTime? validUntil = null)
    {
        ValidateScope(scope);
        ValidateIssuer(sourceType, grantedBy);
        if (validUntil <= grantedAt)
            throw new ArgumentOutOfRangeException(nameof(validUntil), "Grant expiration must be later than its start.");

        return new CapabilityGrant
        {
            CapabilityGrantId = CapabilityGrantId.From(Guid.CreateVersion7()),
            AssignmentId = assignmentId,
            UserId = userId,
            Capability = capability,
            ScopeType = scope.Type,
            ForumId = scope.ForumId,
            CategoryId = scope.CategoryId,
            ThreadId = scope.ThreadId,
            SourceType = sourceType,
            GrantedBy = grantedBy,
            GrantedAt = grantedAt,
            ValidUntil = validUntil
        };
    }

    public bool IsActiveAt(DateTime instant) =>
        RevokedAt is null && GrantedAt <= instant && (ValidUntil is null || ValidUntil > instant);

    public void Revoke(UserId revokedBy, DateTime revokedAt)
    {
        if (RevokedAt is not null) return;
        if (revokedAt < GrantedAt)
            throw new ArgumentOutOfRangeException(nameof(revokedAt), "A grant cannot be revoked before it was issued.");

        RevokedBy = revokedBy;
        RevokedAt = revokedAt;
    }

    public AuthorizationScope GetScope() => ScopeType switch
    {
        AuthorizationScopeType.Platform => AuthorizationScope.Platform,
        AuthorizationScopeType.Forum => AuthorizationScope.Forum(ForumId!.Value),
        AuthorizationScopeType.Category => AuthorizationScope.Category(ForumId!.Value, CategoryId!.Value),
        AuthorizationScopeType.Thread => AuthorizationScope.Thread(ForumId!.Value, CategoryId!.Value, ThreadId!.Value),
        _ => throw new InvalidOperationException("Stored authorization scope is inconsistent.")
    };

    private static void ValidateScope(AuthorizationScope scope)
    {
        var valid = scope.Type switch
        {
            AuthorizationScopeType.Platform =>
                scope.ForumId is null && scope.CategoryId is null && scope.ThreadId is null,
            AuthorizationScopeType.Forum =>
                scope.ForumId is not null && scope.CategoryId is null && scope.ThreadId is null,
            AuthorizationScopeType.Category =>
                scope.ForumId is not null && scope.CategoryId is not null && scope.ThreadId is null,
            AuthorizationScopeType.Thread =>
                scope.ForumId is not null && scope.CategoryId is not null && scope.ThreadId is not null,
            _ => false
        };

        if (!valid) throw new ArgumentException("Authorization scope is inconsistent.", nameof(scope));
    }

    private static void ValidateIssuer(GrantSourceType sourceType, UserId? grantedBy)
    {
        var valid = sourceType == GrantSourceType.PlatformAdministratorBootstrap
            ? grantedBy is null
            : grantedBy is not null;

        if (!valid) throw new ArgumentException("Grant issuer is inconsistent with its source.", nameof(grantedBy));
    }
}
