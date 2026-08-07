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

    public void Add(NotifiableEvent notifiableEvent)
    {
        _dbContext.NotifiableEvents.Add(notifiableEvent);
    }
}
