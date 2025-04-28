using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;

namespace CoreService.Domain.Errors;

public record NonPostAuthorError(ThreadId ThreadId, PostId PostId) : ForbiddenError;