using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Application.Interfaces;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel,
        CancellationToken cancellationToken);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    ValueTask PublishEventAsync<T>(T domainEvent, CancellationToken cancellationToken);

    public Task CommitAsync(CancellationToken cancellationToken);
}