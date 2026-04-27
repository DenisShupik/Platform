using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record NonPostAuthorError(ThreadId ThreadId, PostId PostId) : ForbiddenError;