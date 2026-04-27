using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    NoContent,
    NotFound<ThreadNotFoundError>,
    Forbid<NonThreadOwnerError>,
    Conflict<ThreadNotInStateError>,
    Conflict<ThreadMustContainPostsError>
>;

public static partial class Api
{
    /// <summary>
    /// Запросить одобрение темы
    /// </summary>
    public static async Task<Response> RequestThreadApprovalAsync(
        RequestThreadApprovalRequest request,
        [FromServices] RequestThreadApprovalCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new RequestThreadApprovalCommand
        {
            ThreadId = request.ThreadId,
            RequestedBy = request.RequestedBy.UserId,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error),
            error => new Forbid<NonThreadOwnerError>(error),
            error => TypedResults.Conflict(error),
            error => TypedResults.Conflict(error)
        );
    }
}
