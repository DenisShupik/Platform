using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

public abstract record PolicyRestrictedError(UserId? UserId) : ForbiddenError;

public sealed record ReadPolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);

public sealed record CategoryCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);

public sealed record ThreadCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);

public sealed record PostCreatePolicyRestrictedError(UserId? UserId) : PolicyRestrictedError(UserId);