using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class PostUpdatedEventConsumer(
    IThreadSubscriptionReadRepository threadSubscriptionReadRepository,
    INotifiableEventWriteRepository notificationRepository,
    INotificationWriteRepository notificationDeliveryRepository,
    IUnitOfWork unitOfWork)
{
    public async ValueTask ConsumeAsync(PostUpdatedEvent @event, CancellationToken cancellationToken)
    {
        if (!await threadSubscriptionReadRepository.ExistsExcludingUserAsync(@event.ThreadId, @event.UpdatedBy,
                cancellationToken)) return;

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var notificationPayload = new PostUpdatedNotifiableEventPayload(@event.ThreadId, @event.PostId, @event.UpdatedBy);
        var notification = new NotifiableEvent(notificationPayload, @event.UpdatedAt);

        notificationRepository.Add(notification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationDeliveryRepository.BulkAddAsync(notification.NotifiableEventId,
            @event.ThreadId, @event.UpdatedBy, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
