using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

using DeleteInternalNotificationCommandResult = Result<Success, NotificationNotFoundError>;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class DeleteInternalNotificationCommand : ICommand<DeleteInternalNotificationCommandResult>;

public sealed class DeleteInternalNotificationCommandHandler : ICommandHandler<DeleteInternalNotificationCommand,
    DeleteInternalNotificationCommandResult>
{
    private readonly INotificationWriteRepository _notificationWriteRepository;

    public DeleteInternalNotificationCommandHandler(
        INotificationWriteRepository notificationWriteRepository
    )
    {
        _notificationWriteRepository = notificationWriteRepository;
    }

    public Task<DeleteInternalNotificationCommandResult> HandleAsync(
        DeleteInternalNotificationCommand command, CancellationToken cancellationToken)
    {
        return _notificationWriteRepository.ExecuteRemoveAsync(command.UserId, command.NotifiableEventId,
            ChannelType.Internal, cancellationToken);
    }
}