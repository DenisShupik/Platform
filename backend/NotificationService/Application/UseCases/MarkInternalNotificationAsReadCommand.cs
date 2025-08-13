using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;
using SharedKernel.Application.Interfaces;
using SharedKernel.Domain.Helpers;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId),
    nameof(Notification.NotifiableEventId))]
public sealed partial class MarkInternalNotificationAsReadCommand;

[GenerateOneOf]
public partial class MarkInternalNotificationAsReadCommandResult : OneOfBase<Success, NotificationNotFoundError>;

public sealed class MarkInternalNotificationAsReadCommandHandler
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
        MarkInternalNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notificationOrError = await _notificationWriteRepository.GetOneAsync(request.UserId,
            request.NotifiableEventId, ChannelType.Internal, cancellationToken);

        if (!notificationOrError.TryPickT0(out var notification, out var error)) return error;

        if (notification.DeliveredAt == null)
        {
            notification.DeliveredAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return OneOfHelper.Success;
    }
}