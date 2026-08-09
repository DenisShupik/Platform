using Microsoft.AspNetCore.Mvc;

namespace Shared.Presentation.Errors;

public sealed class ApiValidationProblemDetails : ProblemDetails
{
    public required string Code { get; init; }
    public required IReadOnlyDictionary<string, ApiValidationError> Errors { get; init; }
    public required string TraceId { get; init; }
}
