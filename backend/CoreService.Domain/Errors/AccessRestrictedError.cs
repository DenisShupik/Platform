using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumAccessRestrictedError))]
[JsonDerivedType(typeof(CategoryAccessRestrictedError))]
[JsonDerivedType(typeof(ThreadAccessRestrictedError))]
public abstract record AccessRestrictedError(UserId userId, RestrictionLevel level) : ForbiddenError;

public record ForumAccessRestrictedError(ForumId ForumId, UserId userId, RestrictionLevel level)
    : AccessRestrictedError(userId, level);

public record CategoryAccessRestrictedError(CategoryId CategoryId, UserId userId, RestrictionLevel level)
    : AccessRestrictedError(userId, level);

public record ThreadAccessRestrictedError(ThreadId ThreadId, UserId userId, RestrictionLevel level)
    : AccessRestrictedError(userId, level);