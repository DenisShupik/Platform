using CoreService.Domain.ValueObjects;
using Shared.Domain.Errors;

namespace CoreService.Domain.Errors;

public record NonPostAuthorError(ThreadId ThreadId, PostId PostId) : ForbiddenError;