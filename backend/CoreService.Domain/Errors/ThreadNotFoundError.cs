using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public record ThreadNotFoundError(ThreadId ThreadId) : NotFoundError;