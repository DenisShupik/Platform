using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

/// <summary>
/// Составная read-модель назначения модератора категории. Одно назначение объединяет набор атомарных grants.
/// </summary>
public sealed record CategoryModeratorAppointmentDto
{
    public required AuthorizationAssignmentId AssignmentId { get; init; }
    public required UserId UserId { get; init; }
    public required UserId GrantedBy { get; init; }
    public required DateTime GrantedAt { get; init; }
    public required DateTime? ValidUntil { get; init; }
}
