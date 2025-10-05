using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionReadRepository
{
    Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
    Task<bool> ExistsExcludingUserAsync(ThreadId threadId, UserId? userId, CancellationToken cancellationToken);
}