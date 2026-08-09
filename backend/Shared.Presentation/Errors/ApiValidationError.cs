namespace Shared.Presentation.Errors;

public sealed record ApiValidationError(
    string Code,
    string Message,
    IReadOnlyList<string> Parameters);
