using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Application.Interfaces;

namespace NotificationService.Infrastructure.Consumers;

public sealed class ThreadApprovedEventConsumer(IServiceProvider serviceProvider)
{
    public async ValueTask ConsumeAsync(ThreadApprovedEvent @event, CancellationToken cancellationToken)
    {
        var notifiableEventWriteRepository = serviceProvider.GetRequiredService<INotifiableEventWriteRepository>();
        var notificationWriteRepository = serviceProvider.GetRequiredService<INotificationWriteRepository>();
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        
        var notifiableEvent = new NotifiableEvent(new ThreadApprovedNotifiableEventPayload(@event.ThreadId, @event.CreatedBy,
            @event.ApprovedBy, @event.ApprovedAt), @event.ApprovedAt);

        await notifiableEventWriteRepository.AddAsync(notifiableEvent, cancellationToken);

        var notification = new Notification(@event.CreatedBy, notifiableEvent.NotifiableEventId, ChannelType.Internal);
        await notificationWriteRepository.AddAsync(notification, cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken);
    }
}