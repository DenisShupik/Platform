using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class PostUpdatedEventConsumer(IServiceProvider serviceProvider)
{
    public async ValueTask ConsumeAsync(PostUpdatedEvent @event, CancellationToken cancellationToken)
    {
        var threadSubscriptionReadRepository = serviceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();
        if (!await threadSubscriptionReadRepository.ExistsExcludingUserAsync(@event.ThreadId, @event.UpdatedBy,
                cancellationToken)) return;

        var notificationRepository = serviceProvider.GetRequiredService<INotifiableEventWriteRepository>();
        var notificationDeliveryRepository = serviceProvider.GetRequiredService<INotificationWriteRepository>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var notificationPayload = new PostUpdatedNotifiableEventPayload(@event.ThreadId, @event.PostId, @event.UpdatedBy);
        var notification = new NotifiableEvent(notificationPayload, @event.UpdatedAt);

        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationDeliveryRepository.BulkAddAsync(notification.NotifiableEventId,
            @event.ThreadId, @event.UpdatedBy, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}