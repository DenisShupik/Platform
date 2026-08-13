using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class ThreadSubscriptionWriteRepository : IThreadSubscriptionWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public ThreadSubscriptionWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SuccessOr<DuplicateThreadSubscriptionError>> ExecuteAddAsync(
        ThreadSubscription threadSubscription,
        CancellationToken cancellationToken)
    {
        var insertedCount = await _dbContext.ThreadSubscriptions
            .ToLinqToDBTable()
            .UpsertAsync(threadSubscription, upsert => upsert.SkipUpdate(), cancellationToken);

        return insertedCount == 0
            ? new DuplicateThreadSubscriptionError(threadSubscription.UserId, threadSubscription.ThreadId)
            : SuccessOr.Success;
    }

    public async Task<SuccessOr<ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId,
        ThreadId threadId, CancellationToken cancellationToken)
    {
        var deletedCount = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == userId && e.ThreadId == threadId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount == 0)
            return new ThreadSubscriptionNotFoundError(userId, threadId);

        return SuccessOr.Success;
    }
}
