using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Errors;

namespace CoreService.Domain.Errors;

public record NonThreadOwnerError(ThreadId ThreadId) : ForbiddenError;