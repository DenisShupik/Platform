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

[IncludeAsRequired(typeof(UserNotification), nameof(UserNotification.UserId), nameof(UserNotification.NotificationId))]
public sealed partial class MarkInternalNotificationAsReadCommand;

[GenerateOneOf]
public partial class MarkInternalNotificationAsReadCommandResult : OneOfBase<Success, UserNotificationNotFoundError>;

public sealed class MarkInternalNotificationAsReadCommandHandler
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkInternalNotificationAsReadCommandHandler(
        IUserNotificationRepository userNotificationRepository,
        IUnitOfWork unitOfWork
    )
    {
        _userNotificationRepository = userNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MarkInternalNotificationAsReadCommandResult> HandleAsync(
        MarkInternalNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var userNotificationOrError = await _userNotificationRepository.GetOneAsync(request.UserId,
            request.NotificationId, ChannelType.Internal, cancellationToken);

        if (!userNotificationOrError.TryPickT0(out var userNotification, out var error)) return error;

        if (userNotification.DeliveredAt == null)
        {
            userNotification.DeliveredAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return OneOfHelper.Success;
    }
}