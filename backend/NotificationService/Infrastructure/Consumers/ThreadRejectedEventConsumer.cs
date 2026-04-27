using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class ThreadRejectedEventConsumer(IServiceProvider serviceProvider)
{
    public async ValueTask ConsumeAsync(ThreadRejectedEvent @event, CancellationToken cancellationToken)
    {
        var notifiableEventWriteRepository = serviceProvider.GetRequiredService<INotifiableEventWriteRepository>();
        var notificationWriteRepository = serviceProvider.GetRequiredService<INotificationWriteRepository>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        
        var notifiableEvent = new NotifiableEvent(new ThreadRejectedNotifiableEventPayload(@event.ThreadId, @event.CreatedBy,
            @event.RejectedBy, @event.RejectedAt), @event.RejectedAt);

        await notifiableEventWriteRepository.AddAsync(notifiableEvent, cancellationToken);

        var notification = new Notification(@event.CreatedBy, notifiableEvent.NotifiableEventId, ChannelType.Internal);
        await notificationWriteRepository.AddAsync(notification, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}
