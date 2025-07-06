using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
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

    public async Task<bool> RemoveAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken)
    {
        var rowsAffected = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == userId && e.ThreadId == threadId)
            .ExecuteDeleteAsync(cancellationToken);

        return rowsAffected > 0;
    }
}