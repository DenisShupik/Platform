using NotificationService.Domain.Entities;

namespace NotificationService.Application.Dtos;

public interface IThreadEventProjection
{
    NotifiableEventPayload Payload { get; }
}
