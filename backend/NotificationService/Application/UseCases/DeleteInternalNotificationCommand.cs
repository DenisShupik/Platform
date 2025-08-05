using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using OneOf;
using OneOf.Types;

namespace NotificationService.Application.UseCases;

[Include(typeof(UserNotification), PropertyGenerationMode.AsRequired, nameof(UserNotification.UserId),
    nameof(UserNotification.NotificationId))]
public sealed partial class DeleteInternalNotificationCommand;

public sealed class DeleteInternalNotificationCommandHandler
{
    private readonly IUserNotificationRepository _userNotificationRepository;

    public DeleteInternalNotificationCommandHandler(
        IUserNotificationRepository userNotificationRepository
    )
    {
        _userNotificationRepository = userNotificationRepository;
    }

    public Task<OneOf<Success, UserNotificationNotFoundError>> HandleAsync(
        DeleteInternalNotificationCommand request, CancellationToken cancellationToken)
    {
        return _userNotificationRepository.ExecuteRemoveAsync(request.UserId, request.NotificationId,
            ChannelType.Internal, cancellationToken);
    }
}