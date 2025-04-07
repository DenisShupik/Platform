using SharedKernel.Domain.Errors;
using SharedKernel.Domain.ValueObjects;

namespace UserService.Domain.Errors;

public record UserNotFoundError(UserId UserId) : NotFoundError;