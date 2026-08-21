using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed record CapabilityGrantDto
{
    public required CapabilityGrantId CapabilityGrantId { get; init; }
    public required AuthorizationAssignmentId AssignmentId { get; init; }
    public required UserId UserId { get; init; }
    public required CapabilityCode Capability { get; init; }
    public required AuthorizationScopeType ScopeType { get; init; }
    public required ForumId? ForumId { get; init; }
    public required CategoryId? CategoryId { get; init; }
    public required ThreadId? ThreadId { get; init; }
    public required GrantSourceType SourceType { get; init; }
    public required UserId? GrantedBy { get; init; }
    public required DateTime GrantedAt { get; init; }
    public required DateTime? ValidUntil { get; init; }
    public required UserId? RevokedBy { get; init; }
    public required DateTime? RevokedAt { get; init; }
}
