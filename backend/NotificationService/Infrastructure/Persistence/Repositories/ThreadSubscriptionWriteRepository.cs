using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class ThreadSubscriptionWriteRepository : IThreadSubscriptionWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public ThreadSubscriptionWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ThreadSubscription threadSubscription, CancellationToken cancellationToken)
    {
        await _dbContext.ThreadSubscriptions.AddAsync(threadSubscription, cancellationToken);
    }

    public async Task<Result<Success, ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId,
        ThreadId threadId, CancellationToken cancellationToken)
    {
        var deletedCount = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == userId && e.ThreadId == threadId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount == 0)
            return new ThreadSubscriptionNotFoundError(userId, threadId);

        return Success.Instance;
    }
}