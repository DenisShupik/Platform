using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Exceptions;

public sealed class UnauthorizedException: Exception
{
    public AuthenticationError Error { get; }

    public UnauthorizedException(AuthenticationError error)
    {
        Error = error;
    }
}
