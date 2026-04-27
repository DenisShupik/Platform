using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Errors;

public record ClaimNotFoundError(string ClaimName) : AuthenticationError;