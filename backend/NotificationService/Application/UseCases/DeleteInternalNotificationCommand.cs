using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class DeleteInternalNotificationCommand : ICommand<OneOf<Success, NotificationNotFoundError>>;

public sealed class DeleteInternalNotificationCommandHandler : ICommandHandler<DeleteInternalNotificationCommand,
    OneOf<Success, NotificationNotFoundError>>
{
    private readonly INotificationWriteRepository _notificationWriteRepository;

    public DeleteInternalNotificationCommandHandler(
        INotificationWriteRepository notificationWriteRepository
    )
    {
        _notificationWriteRepository = notificationWriteRepository;
    }

    public Task<OneOf<Success, NotificationNotFoundError>> HandleAsync(
        DeleteInternalNotificationCommand command, CancellationToken cancellationToken)
    {
        return _notificationWriteRepository.ExecuteRemoveAsync(command.UserId, command.NotifiableEventId,
            ChannelType.Internal, cancellationToken);
    }
}