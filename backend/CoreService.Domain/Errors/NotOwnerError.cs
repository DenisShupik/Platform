using Shared.Domain.Errors;

namespace CoreService.Domain.Errors;

public record NotOwnerError : ForbiddenError;