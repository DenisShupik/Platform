using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;

namespace CoreService.Domain.Errors;

public record PostStaleError(ThreadId ThreadId, PostId PostId, uint RowVersion) : ConflictError;