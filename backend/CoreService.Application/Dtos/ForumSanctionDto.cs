using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed record ForumSanctionDto
{
    public required ForumSanctionId ForumSanctionId { get; init; }
    public required UserId UserId { get; init; }
    public required ForumSanctionType Type { get; init; }
    public required AuthorizationScopeType ScopeType { get; init; }
    public required ForumId? ForumId { get; init; }
    public required CategoryId? CategoryId { get; init; }
    public required ThreadId? ThreadId { get; init; }
    public required ForumSanctionReason Reason { get; init; }
    public required UserId IssuedBy { get; init; }
    public required DateTime IssuedAt { get; init; }
    public required DateTime? ValidUntil { get; init; }
    public required UserId? RevokedBy { get; init; }
    public required DateTime? RevokedAt { get; init; }
}
