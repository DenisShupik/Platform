using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public record ThreadAccessRestrictedError(ThreadId ThreadId, UserId userId) : ForbiddenError;