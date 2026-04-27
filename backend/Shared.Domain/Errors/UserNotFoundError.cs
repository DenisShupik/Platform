using Shared.Domain.Abstractions.Errors;

namespace Shared.Domain.Errors;

public sealed record UserNotFoundError : NotFoundError;