using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public record RoleNotFoundError : NotFoundError;