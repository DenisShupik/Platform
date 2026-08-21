using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Временное или бессрочное ограничение действий пользователя в области форума.
/// </summary>
public sealed class ForumSanction
{
    public ForumSanctionId ForumSanctionId { get; private set; }
    public UserId UserId { get; private set; }
    public ForumSanctionType Type { get; private set; }
    public AuthorizationScopeType ScopeType { get; private set; }
    public ForumId? ForumId { get; private set; }
    public CategoryId? CategoryId { get; private set; }
    public ThreadId? ThreadId { get; private set; }
    public ForumSanctionReason Reason { get; private set; }
    public UserId IssuedBy { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime? ValidUntil { get; private set; }
    public UserId? RevokedBy { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private ForumSanction()
    {
    }

    public static ForumSanction Issue(
        UserId userId,
        ForumSanctionType type,
        AuthorizationScope scope,
        ForumSanctionReason reason,
        UserId issuedBy,
        DateTime issuedAt,
        DateTime? validUntil)
    {
        if (validUntil <= issuedAt)
            throw new ArgumentOutOfRangeException(nameof(validUntil), "Sanction expiration must be later than its start.");

        return new ForumSanction
        {
            ForumSanctionId = ForumSanctionId.From(Guid.CreateVersion7()),
            UserId = userId,
            Type = type,
            ScopeType = scope.Type,
            ForumId = scope.ForumId,
            CategoryId = scope.CategoryId,
            ThreadId = scope.ThreadId,
            Reason = reason,
            IssuedBy = issuedBy,
            IssuedAt = issuedAt,
            ValidUntil = validUntil
        };
    }

    public bool IsActiveAt(DateTime instant) =>
        RevokedAt is null && IssuedAt <= instant && (ValidUntil is null || ValidUntil > instant);

    public bool RestrictsReadingAt(DateTime instant) =>
        Type == ForumSanctionType.NoAccess && IsActiveAt(instant);

    public bool RestrictsParticipationAt(DateTime instant) => IsActiveAt(instant);

    public AuthorizationScope GetScope() => ScopeType switch
    {
        AuthorizationScopeType.Platform => AuthorizationScope.Platform,
        AuthorizationScopeType.Forum => AuthorizationScope.Forum(ForumId!.Value),
        AuthorizationScopeType.Category => AuthorizationScope.Category(ForumId!.Value, CategoryId!.Value),
        AuthorizationScopeType.Thread => AuthorizationScope.Thread(ForumId!.Value, CategoryId!.Value, ThreadId!.Value),
        _ => throw new InvalidOperationException("Stored sanction scope is inconsistent.")
    };

    public void Revoke(UserId revokedBy, DateTime revokedAt)
    {
        if (RevokedAt is not null) return;
        if (revokedAt < IssuedAt)
            throw new ArgumentOutOfRangeException(nameof(revokedAt), "A sanction cannot be revoked before it was issued.");

        RevokedBy = revokedBy;
        RevokedAt = revokedAt;
    }
}
