using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public record PostCreatePolicyViolationError(ThreadId ThreadId, UserId? UserId, Policy Policy) : ForbiddenError;