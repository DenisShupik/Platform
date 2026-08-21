using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed record PlatformAdministratorAppointmentDto
{
    public required AuthorizationAssignmentId AssignmentId { get; init; }
    public required UserId UserId { get; init; }
    public required UserId? GrantedBy { get; init; }
    public required DateTime GrantedAt { get; init; }
    public required bool WasBootstrapped { get; init; }
}
