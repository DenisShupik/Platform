using Shared.Domain.Abstractions.Errors;

namespace NotificationService.Domain.Errors;

public sealed record PermissionDeniedError : ForbiddenError;
