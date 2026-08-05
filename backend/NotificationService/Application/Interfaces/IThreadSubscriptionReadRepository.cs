using CoreService.Domain.ValueObjects;
using NotificationService.Application.UseCases;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionReadRepository
{
    Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
    Task<bool> ExistsExcludingUserAsync(ThreadId threadId, UserId? userId, CancellationToken cancellationToken);

    Task<PagedList<ThreadId>> GetSubscribedThreadIdsAsync(GetThreadSubscriptionsPagedQuery query,
        CancellationToken cancellationToken);

    Task<List<T>> GetLatestEventsAsync<T>(GetThreadSubscriptionLatestEventsPagedQuery<T> query,
        CancellationToken cancellationToken);
}
