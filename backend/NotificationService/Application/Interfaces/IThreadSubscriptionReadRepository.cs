using CoreService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionReadRepository
{
    public Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
    public Task<bool> ExistsExcludingUserAsync(ThreadId threadId,UserId userId, CancellationToken cancellationToken);
}