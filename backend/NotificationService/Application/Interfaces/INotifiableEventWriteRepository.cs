using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotifiableEventWriteRepository
{
    public Task AddAsync(NotifiableEvent notifiableEvent, CancellationToken cancellationToken);
}