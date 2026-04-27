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
    /// Отклонить тему
    /// </summary>
    public static async Task<Response> RejectThreadAsync(
        RejectThreadRequest request,
        [FromServices] RejectThreadCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new RejectThreadCommand
        {
            ThreadId = request.ThreadId,
            RejectedBy = request.RequestedBy.UserId,
            RejectedAt = DateTime.UtcNow,
            RejecterRole = request.RequestedBy.Role,
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
