using CoreService.Domain.ValueObjects;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class ThreadSubscriptionRepository : IThreadSubscriptionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ThreadSubscriptionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ThreadSubscription threadSubscription, CancellationToken cancellationToken)
    {
        await _dbContext.ThreadSubscriptions.AddAsync(threadSubscription, cancellationToken);
    }

    public void Remove(UserId userId, ThreadId threadId, CancellationToken cancellationToken)
    {
        _dbContext.ThreadSubscriptions.Remove(new ThreadSubscription(userId, threadId));
    }
}