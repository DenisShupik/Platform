using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record ForumNotFoundError(ForumId ForumId) : NotFoundError;