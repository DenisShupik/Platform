using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumAccessLevelError))]
[JsonDerivedType(typeof(CategoryAccessLevelError))]
[JsonDerivedType(typeof(ThreadAccessLevelError))]
public abstract record AccessLevelError(UserId? userId, AccessLevel level) : ForbiddenError;

public record ForumAccessLevelError(ForumId ForumId, UserId? userId, AccessLevel level)
    : AccessLevelError(userId, level);

public record CategoryAccessLevelError(CategoryId CategoryId, UserId? userId, AccessLevel level)
    : AccessLevelError(userId, level);

public record ThreadAccessLevelError(ThreadId ThreadId, UserId? userId, AccessLevel level)
    : AccessLevelError(userId, level);