using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadRepository : IThreadRepository
{
    private readonly WritableApplicationDbContext _dbContext;

    public ThreadRepository(WritableApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ThreadNotFoundError>> GetWithLockAsync<T>(
        ThreadId threadId,
        CancellationToken cancellationToken
    )
        where T : class, IHasThreadId
    {
        var thread = await _dbContext.Set<T>()
            .Where(e => e.ThreadId == threadId)
            .QueryHint(PostgreSQLHints.ForUpdate)
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (thread == null) return new ThreadNotFoundError(threadId);

        return thread;
    }
}