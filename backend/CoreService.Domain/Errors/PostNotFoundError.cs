using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;

namespace CoreService.Domain.Errors;

public record PostNotFoundError(ThreadId ThreadId, PostId PostId) : NotFoundError;