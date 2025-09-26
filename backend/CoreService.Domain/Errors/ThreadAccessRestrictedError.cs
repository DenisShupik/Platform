using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public record ThreadAccessRestrictedError(ThreadId ThreadId, UserId userId, RestrictionLevel level) : ForbiddenError;