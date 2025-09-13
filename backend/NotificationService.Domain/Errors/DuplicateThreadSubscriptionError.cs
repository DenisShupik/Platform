using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record DuplicateThreadSubscriptionError(UserId UserId, ThreadId ThreadId) : ConflictError;