using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;
using SharedKernel.Domain.Helpers;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class ThreadSubscriptionWriteRepository : IThreadSubscriptionWriteRepository
{
    private readonly WritableApplicationDbContext _dbContext;

    public ThreadSubscriptionWriteRepository(WritableApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ThreadSubscription threadSubscription, CancellationToken cancellationToken)
    {
        await _dbContext.ThreadSubscriptions.AddAsync(threadSubscription, cancellationToken);
    }

    public async Task<OneOf<Success, ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId,
        ThreadId threadId, CancellationToken cancellationToken)
    {
        var deletedCount = await _dbContext.ThreadSubscriptions
            .Where(e => e.UserId == userId && e.ThreadId == threadId)
            .ExecuteDeleteAsync(cancellationToken);

        if (deletedCount == 0)
            return new ThreadSubscriptionNotFoundError(userId, threadId);

        return OneOfHelper.Success;
    }
}