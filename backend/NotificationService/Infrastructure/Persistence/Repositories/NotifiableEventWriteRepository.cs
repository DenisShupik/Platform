using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class NotifiableEventWriteRepository : INotifiableEventWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public NotifiableEventWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotifiableEvent notifiableEvent, CancellationToken cancellationToken)
    {
        await _dbContext.NotifiableEvents.AddAsync(notifiableEvent, cancellationToken);
    }
}