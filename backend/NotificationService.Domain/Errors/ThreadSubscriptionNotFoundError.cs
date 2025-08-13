using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record ThreadSubscriptionNotFoundError(UserId UserId, ThreadId ThreadId) : NotFoundError;