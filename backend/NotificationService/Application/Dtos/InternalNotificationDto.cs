using NotificationService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.Dtos;

[Include(typeof(NotifiableEvent), PropertyGenerationMode.AsPublic, nameof(NotifiableEvent.Payload),
    nameof(NotifiableEvent.OccurredAt))]
[Include(typeof(Notification), PropertyGenerationMode.AsPublic, nameof(Notification.NotifiableEventId),
    nameof(Notification.DeliveredAt))]
public sealed partial class InternalNotificationDto;