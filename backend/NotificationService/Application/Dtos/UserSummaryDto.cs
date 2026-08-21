using Shared.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Dtos;

public sealed class UserSummaryDto
{
    public required UserId UserId { get; init; }
    public required Username Username { get; init; }
}
