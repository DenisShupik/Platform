using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Wolverine.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence;

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
    
    public ValueTask PublishEventAsync<T>(T @event, CancellationToken cancellationToken) where T : IDomainEvent
    {
        return _outbox.PublishAsync(@event);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }
}