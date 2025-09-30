using System.Text.Json.Serialization;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumAccessLevelError))]
[JsonDerivedType(typeof(CategoryAccessLevelError))]
[JsonDerivedType(typeof(ThreadAccessLevelError))]
public abstract record AccessLevelError(UserId? UserId, AccessLevel Level) : ForbiddenError;

public record ForumAccessLevelError(ForumId ForumId, UserId? UserId, AccessLevel Level)
    : AccessLevelError(UserId, Level);

public record CategoryAccessLevelError(CategoryId CategoryId, UserId? UserId, AccessLevel Level)
    : AccessLevelError(UserId, Level);

public record ThreadAccessLevelError(ThreadId ThreadId, UserId? UserId, AccessLevel Level)
    : AccessLevelError(UserId, Level);

