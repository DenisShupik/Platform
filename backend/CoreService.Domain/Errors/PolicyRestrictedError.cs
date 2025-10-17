using CoreService.Domain.Enums;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public sealed record PolicyRestrictedError(PolicyType PolicyType, UserId? UserId) : ForbiddenError;