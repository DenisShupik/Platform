using NotificationService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

public sealed class NotifiableEvent
{
    /// <summary>
    /// Идентификатор события
    /// </summary>
    public NotifiableEventId NotifiableEventId { get; private set; }

    /// <summary>
    /// Данные события
    /// </summary>
    public NotifiableEventPayload Payload { get; private set; }

    /// <summary>
    /// Дата и время события
    /// </summary>
    public DateTime OccurredAt { get; private set; }

    public NotifiableEvent(NotifiableEventPayload payload, DateTime occurredAt)
    {
        NotifiableEventId = NotifiableEventId.From(Guid.CreateVersion7());
        Payload = payload;
        OccurredAt = occurredAt;
    }
}