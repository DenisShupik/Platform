using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record ThreadSubscriptionNotFoundError(UserId UserId, ThreadId ThreadId) : NotFoundError;