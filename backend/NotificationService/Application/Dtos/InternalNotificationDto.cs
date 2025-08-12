using Generator.Attributes;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Dtos;

[Include(typeof(NotifiableEvent), PropertyGenerationMode.AsPublic, nameof(NotifiableEvent.Payload),
    nameof(NotifiableEvent.OccurredAt))]
[Include(typeof(Notification), PropertyGenerationMode.AsPublic, nameof(Notification.NotifiableEventId),
    nameof(Notification.DeliveredAt))]
public sealed partial class InternalNotificationDto;