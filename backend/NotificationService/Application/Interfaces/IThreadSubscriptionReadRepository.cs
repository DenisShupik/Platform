using CoreService.Domain.ValueObjects;
using NotificationService.Application.UseCases;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionReadRepository
{
    Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
    Task<bool> ExistsExcludingUserAsync(ThreadId threadId, UserId? userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<T>> GetLatestEventPerThreadAsync<T>(GetWatchedThreadLatestEventPagedQuery<T> query,
        CancellationToken cancellationToken);
}