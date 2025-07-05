using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionRepository
{
    public Task AddAsync(ThreadSubscription threadSubscription, CancellationToken cancellationToken);
    public void Remove(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
}