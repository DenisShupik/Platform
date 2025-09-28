using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public record NonThreadOwnerError(ThreadId ThreadId) : ForbiddenError;