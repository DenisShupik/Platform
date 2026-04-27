using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Errors;

public sealed record TokenExpiredError(string Message) : AuthenticationError;