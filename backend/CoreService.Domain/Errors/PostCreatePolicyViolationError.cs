using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public record PostCreatePolicyViolationError(ThreadId ThreadId, PostCreatePolicy Policy) : ForbiddenError;