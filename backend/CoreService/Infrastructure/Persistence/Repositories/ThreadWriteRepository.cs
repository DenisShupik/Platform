using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Shared.Application.Enums;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadWriteWriteRepository : IThreadWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public ThreadWriteWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Thread, ThreadNotFoundError>> GetOneAsync(ThreadId threadId, LockMode lockMode,
        CancellationToken cancellationToken)
    {
        var thread = await _dbContext.Threads
            .Where(e => e.ThreadId == threadId)
            .WithLockLinq2Db(lockMode)
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (thread == null) return new ThreadNotFoundError();

        return thread;
    }

    public void Add(Thread thread)
    {
        _dbContext.Threads.Add(thread);
    }
}