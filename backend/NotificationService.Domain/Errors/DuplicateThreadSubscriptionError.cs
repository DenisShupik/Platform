using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using Shared.Domain.ValueObjects;

namespace NotificationService.Domain.Errors;

public record DuplicateThreadSubscriptionError(UserId UserId, ThreadId ThreadId) : ConflictError;