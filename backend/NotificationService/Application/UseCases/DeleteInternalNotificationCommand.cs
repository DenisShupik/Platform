using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class DeleteInternalNotificationCommand;

public sealed class DeleteInternalNotificationCommandHandler
{
    private readonly INotificationWriteRepository _notificationWriteRepository;

    public DeleteInternalNotificationCommandHandler(
        INotificationWriteRepository notificationWriteRepository
    )
    {
        _notificationWriteRepository = notificationWriteRepository;
    }

    public Task<OneOf<Success, NotificationNotFoundError>> HandleAsync(
        DeleteInternalNotificationCommand request, CancellationToken cancellationToken)
    {
        return _notificationWriteRepository.ExecuteRemoveAsync(request.UserId, request.NotifiableEventId,
            ChannelType.Internal, cancellationToken);
    }
}