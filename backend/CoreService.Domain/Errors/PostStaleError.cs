using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record PostStaleError(ThreadId ThreadId, PostId PostId, uint RowVersion) : ConflictError;