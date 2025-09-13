namespace Shared.Domain.Interfaces;

public interface IDomainEventBus
{
    public void AddEvent(IDomainEvent domainEvent);
}