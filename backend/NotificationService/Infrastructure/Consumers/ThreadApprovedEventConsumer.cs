using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class ThreadApprovedEventConsumer(
    INotifiableEventWriteRepository notifiableEventWriteRepository,
    INotificationWriteRepository notificationWriteRepository,
    IUnitOfWork unitOfWork)
{
    public async ValueTask ConsumeAsync(ThreadApprovedEvent @event, CancellationToken cancellationToken)
    {
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        
        var notifiableEvent = new NotifiableEvent(new ThreadApprovedNotifiableEventPayload(@event.ThreadId, @event.CreatedBy,
            @event.ApprovedBy, @event.ApprovedAt), @event.ApprovedAt);

        notifiableEventWriteRepository.Add(notifiableEvent);

        var notification = new Notification(@event.CreatedBy, notifiableEvent.NotifiableEventId, ChannelType.Internal);
        notificationWriteRepository.Add(notification);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
