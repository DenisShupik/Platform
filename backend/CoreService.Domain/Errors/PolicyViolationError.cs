using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record PolicyViolationError(PolicyId PolicyId, UserId? UserId) : ForbiddenError;

