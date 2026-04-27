using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<ThreadDto>,
    NotFound<ThreadNotFoundError>,
    Forbid<PermissionDeniedError>,
    Forbid<NonThreadOwnerError>
>;

public static partial class Api
{
    /// <summary>
    /// Получить тему
    /// </summary>
    public static async Task<Response>
        GetThreadAsync(
            GetThreadRequest request,
            [FromServices] GetThreadQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetThreadQuery<ThreadDto>
        {
            ThreadId = request.ThreadId,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PermissionDeniedError>(error)
        );
    }
}