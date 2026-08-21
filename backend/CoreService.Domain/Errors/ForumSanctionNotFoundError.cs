using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record ForumSanctionNotFoundError(ForumSanctionId ForumSanctionId) : NotFoundError;
