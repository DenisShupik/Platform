using Shared.Domain.Errors;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.Errors;

public record UserNotFoundError(UserId UserId) : NotFoundError;