using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumPolicyRestrictedError))]
[JsonDerivedType(typeof(CategoryPolicyRestrictedError))]
[JsonDerivedType(typeof(ThreadPolicyRestrictedError))]
public abstract record PolicyRestrictedError(UserId? UserId, PolicyType Policy) : ForbiddenError;

public record ForumPolicyRestrictedError(ForumId ForumId, UserId? UserId, PolicyType Policy)
    : PolicyRestrictedError(UserId, Policy);

public record CategoryPolicyRestrictedError(CategoryId CategoryId, UserId? UserId, PolicyType Policy)
    : PolicyRestrictedError(UserId, Policy);

public record ThreadPolicyRestrictedError(ThreadId ThreadId, UserId? UserId, PolicyType Policy)
    : PolicyRestrictedError(UserId, Policy);