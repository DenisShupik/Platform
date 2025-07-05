using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class ThreadSubscriptionReadRepository : IThreadSubscriptionReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ThreadSubscriptionReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public Task<bool> ExistsAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken)
    {
        return _dbContext.ThreadSubscriptions
            .AnyAsyncEF(e => e.UserId == userId && e.ThreadId == threadId, cancellationToken);
    }
}