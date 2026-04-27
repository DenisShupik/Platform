using Shared.Domain.Abstractions.Errors;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Domain.Errors;

public sealed record ThreadNotInStateError(ThreadState State) : ConflictError;