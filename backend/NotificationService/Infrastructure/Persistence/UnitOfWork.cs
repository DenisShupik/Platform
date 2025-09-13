using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Interfaces;
using Wolverine.EntityFrameworkCore;

namespace NotificationService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbContextOutbox<WriteApplicationDbContext> _outbox;

    public UnitOfWork(IDbContextOutbox<WriteApplicationDbContext> outbox)
    {
        _outbox = outbox;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        return _outbox.DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _outbox.DbContext.SaveChangesAsync(cancellationToken);
    }
    
    public ValueTask PublishEventAsync<T>(T domainEvent, CancellationToken cancellationToken)
    {
        return _outbox.PublishAsync(domainEvent);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }
}