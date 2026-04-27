namespace Shared.Presentation.Exceptions;

public sealed class ValidationException : Exception
{
    public readonly IReadOnlyDictionary<string, string> Errors;

    public ValidationException(IReadOnlyDictionary<string, string> errors)
    {
        Errors = errors;
    }
}