using System.Text.Json.Serialization;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Errors;

[JsonPolymorphic]
[JsonDerivedType(typeof(ForumModerationForbiddenError))]
[JsonDerivedType(typeof(CategoryModerationForbiddenError))]
[JsonDerivedType(typeof(ThreadModerationForbiddenError))]
public abstract record ModerationForbiddenError(UserId UserId) : ForbiddenError;

public record ForumModerationForbiddenError(UserId UserId, ForumId ForumId) : ModerationForbiddenError(UserId);

public record CategoryModerationForbiddenError(UserId UserId, CategoryId CategoryId) : ModerationForbiddenError(UserId);

public record ThreadModerationForbiddenError(UserId UserId, ThreadId ThreadId) : ModerationForbiddenError(UserId);