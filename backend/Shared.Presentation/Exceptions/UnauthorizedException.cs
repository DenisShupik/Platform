using Shared.Domain.Abstractions.Errors;

namespace Shared.Presentation.Exceptions;

public sealed class UnauthorizedException: Exception
{
    private readonly AuthenticationError _error;

    public UnauthorizedException(AuthenticationError error)
    {
        _error = error;
    }
}