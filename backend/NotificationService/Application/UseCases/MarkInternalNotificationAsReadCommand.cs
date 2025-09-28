using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using OneOf;
using Shared.TypeGenerator.Attributes;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class MarkInternalNotificationAsReadCommand : ICommand<OneOf<Success, NotificationNotFoundError>>;

public sealed class MarkInternalNotificationAsReadCommandHandler : ICommandHandler<MarkInternalNotificationAsReadCommand
    , OneOf<Success, NotificationNotFoundError>>
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

    public async Task<OneOf<Success, NotificationNotFoundError>> HandleAsync(
        MarkInternalNotificationAsReadCommand command, CancellationToken cancellationToken)
    {
        var notificationOrError = await _notificationWriteRepository.GetOneAsync(command.UserId,
            command.NotifiableEventId, ChannelType.Internal, cancellationToken);

        if (!notificationOrError.TryPickT0(out var notification, out var error)) return error;

        if (notification.DeliveredAt == null)
        {
            notification.DeliveredAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Success.Instance;
    }
}