using Microsoft.AspNetCore.Mvc;

namespace Shared.Presentation.Errors;

public sealed class ApiProblemDetails : ProblemDetails
{
    public required string Code { get; init; }
    public required string TraceId { get; init; }
}
