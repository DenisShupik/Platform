using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ThreadWriteWriteRepository : IThreadWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public ThreadWriteWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Thread, ThreadNotFoundError>> GetOneAsync(ThreadId threadId,
        CancellationToken cancellationToken)
    {
        var thread = await _dbContext.Threads
            .Where(e => e.ThreadId == threadId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (thread == null) return new ThreadNotFoundError(threadId);

        return thread;
    }
    
    public async Task<Result<ThreadPostAddable, ThreadNotFoundError>> GetThreadPostAddableAsync(ThreadId threadId,
        CancellationToken cancellationToken)
    {
        var thread = await _dbContext.ThreadPostAddable
            .Where(e => e.ThreadId == threadId)
            .QueryHint(PostgreSQLHints.ForUpdate)
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (thread == null) return new ThreadNotFoundError(threadId);

        return thread;
    }
}