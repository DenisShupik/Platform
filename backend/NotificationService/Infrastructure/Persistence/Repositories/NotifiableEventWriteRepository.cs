using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Repositories;

public sealed class NotifiableEventWriteRepository : INotifiableEventWriteRepository
{
    private readonly WritableApplicationDbContext _dbContext;

    public NotifiableEventWriteRepository(WritableApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(NotifiableEvent notifiableEvent, CancellationToken cancellationToken)
    {
        await _dbContext.NotifiableEvents.AddAsync(notifiableEvent, cancellationToken);
    }
}