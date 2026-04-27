using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    NoContent,
    Forbid<PermissionDeniedError>,
    NotFound<ThreadNotFoundError>,
    Conflict<ThreadNotInStateError>
>;

public static partial class Api
{
    /// <summary>
    /// Одобрить тему
    /// </summary>
    public static async Task<Response> ApproveThreadAsync(
        ApproveThreadRequest request,
        [FromServices] ApproveThreadCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new ApproveThreadCommand
        {
            ThreadId = request.ThreadId,
            ApprovedBy = request.RequestedBy.UserId,
            ApprovedAt = DateTime.UtcNow,
            ApproverRole = request.RequestedBy.Role,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => new Forbid<PermissionDeniedError>(error),
            error => TypedResults.NotFound(error),
            error => TypedResults.Conflict(error)
        );
    }
}