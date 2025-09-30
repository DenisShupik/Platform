using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace NotificationService.Application.UseCases;

using MarkInternalNotificationAsReadCommandResult = Result<Success, NotificationNotFoundError>;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class
    MarkInternalNotificationAsReadCommand : ICommand<MarkInternalNotificationAsReadCommandResult>;

public sealed class MarkInternalNotificationAsReadCommandHandler : ICommandHandler<MarkInternalNotificationAsReadCommand
    , MarkInternalNotificationAsReadCommandResult>
{
    private readonly INotificationWriteRepository _notificationWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkInternalNotificationAsReadCommandHandler(
        INotificationWriteRepository notificationWriteRepository,
        IUnitOfWork unitOfWork
    )
    {
        _notificationWriteRepository = notificationWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarkInternalNotificationAsReadCommandResult> HandleAsync(
        MarkInternalNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        var notificationOrError = await _notificationWriteRepository.GetOneAsync(command.UserId,
            command.NotifiableEventId, ChannelType.Internal, cancellationToken);

        if (!notificationOrError.TryGet(out var notification, out var error)) return error;

        if (notification.DeliveredAt == null)
        {
            notification.DeliveredAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Success.Instance;
    }
}