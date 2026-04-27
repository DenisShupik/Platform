using NotificationService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.Dtos;

[Include(typeof(NotifiableEvent), PropertyGenerationMode.AsPublic, nameof(NotifiableEvent.NotifiableEventId),
    nameof(NotifiableEvent.Payload), nameof(NotifiableEvent.OccurredAt))]
public sealed partial class WatchedThreadLatestEventDto;