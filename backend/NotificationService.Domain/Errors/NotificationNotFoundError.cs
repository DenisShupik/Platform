using NotificationService.Domain.Enums;
using NotificationService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record NotificationNotFoundError(UserId UserId, NotifiableEventId NotifiableEventId, ChannelType Channel) : NotFoundError;