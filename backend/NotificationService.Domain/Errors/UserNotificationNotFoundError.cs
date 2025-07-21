using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record UserNotificationNotFoundError(UserId UserId, NotificationId NotificationId, ChannelType Channel) : NotFoundError;