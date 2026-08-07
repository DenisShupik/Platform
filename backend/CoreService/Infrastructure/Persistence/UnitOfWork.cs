using System.Data;
using CoreService.Domain.Entities;
using CoreService.Infrastructure.Markdown;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Application.Interfaces;
using Shared.Domain.Interfaces;
using Wolverine.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbContextOutbox<WriteApplicationDbContext> _outbox;
    private readonly IPostSearchTextProjector _postSearchTextProjector;

    public UnitOfWork(
        IDbContextOutbox<WriteApplicationDbContext> outbox,
        IPostSearchTextProjector postSearchTextProjector)
    {
        _outbox = outbox;
        _postSearchTextProjector = postSearchTextProjector;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
        CancellationToken cancellationToken)
    {
        return _outbox.DbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        ProjectPostSearchText();
        return _outbox.DbContext.SaveChangesAsync(cancellationToken);
    }

    public ValueTask PublishEventAsync<T>(T @event, CancellationToken cancellationToken) where T : IDomainEvent
    {
        return _outbox.PublishAsync(@event);
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        ProjectPostSearchText();
        return _outbox.SaveChangesAndFlushMessagesAsync(cancellationToken);
    }

    private void ProjectPostSearchText()
    {
        _outbox.DbContext.ChangeTracker.DetectChanges();

        foreach (var entry in _outbox.DbContext.ChangeTracker.Entries<Post>())
        {
            if (entry.State != EntityState.Added &&
                (entry.State != EntityState.Modified || !entry.Property(post => post.Content).IsModified)) continue;

            entry.Property<string>(Constants.SearchTextPropertyName).CurrentValue =
                _postSearchTextProjector.Project(entry.Entity.Content);
        }
    }
}
