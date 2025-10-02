using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumAccessPolicyViolationError))]
[JsonDerivedType(typeof(CategoryAccessPolicyViolationError))]
[JsonDerivedType(typeof(ThreadAccessPolicyViolationError))]
public abstract record AccessPolicyViolationError(UserId? UserId, Policy Policy) : ForbiddenError;

public record ForumAccessPolicyViolationError(ForumId ForumId, UserId? UserId, Policy Policy)
    : AccessPolicyViolationError(UserId, Policy);

public record CategoryAccessPolicyViolationError(CategoryId CategoryId, UserId? UserId, Policy Policy)
    : AccessPolicyViolationError(UserId, Policy);

public record ThreadAccessPolicyViolationError(ThreadId ThreadId, UserId? UserId, Policy Policy)
    : AccessPolicyViolationError(UserId, Policy);

