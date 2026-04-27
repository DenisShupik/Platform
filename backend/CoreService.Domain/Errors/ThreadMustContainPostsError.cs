using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record ThreadMustContainPostsError() : ConflictError;