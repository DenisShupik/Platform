using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class PostAddedEventConsumer(IServiceProvider serviceProvider)
{
    public async ValueTask ConsumeAsync(PostAddedEvent @event, CancellationToken cancellationToken)
    {
        var threadSubscriptionReadRepository = serviceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();
        if (!await threadSubscriptionReadRepository.ExistsExcludingUserAsync(@event.ThreadId, @event.CreatedBy,
                cancellationToken)) return;

        var notificationRepository = serviceProvider.GetRequiredService<INotifiableEventWriteRepository>();
        var notificationDeliveryRepository = serviceProvider.GetRequiredService<INotificationWriteRepository>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        var notificationPayload = new PostAddedNotifiableEventPayload(@event.ThreadId, @event.PostId, @event.CreatedBy);
        var notification = new NotifiableEvent(notificationPayload, @event.CreatedAt);

        await notificationRepository.AddAsync(notification, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await notificationDeliveryRepository.BulkAddAsync(notification.NotifiableEventId,
            @event.ThreadId, @event.CreatedBy, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }
}