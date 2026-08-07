using NotificationService.Domain.Entities;

namespace NotificationService.Application.Interfaces;

public interface INotifiableEventWriteRepository
{
    public void Add(NotifiableEvent notifiableEvent);
}
