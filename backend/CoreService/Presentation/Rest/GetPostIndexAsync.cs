using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Index = Shared.Domain.ValueObjects.Index;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<Index>,
    NotFound<PostNotFoundError>,
    Forbid<PermissionDeniedError>
>;

public static partial class Api
{
    /// <summary>
    /// Получить индекс сообщения
    /// </summary>
    public static async Task<Response> GetPostIndexAsync(
        GetPostIndexRequest request,
        [FromServices] GetPostIndexQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new GetPostIndexQuery
        {
            PostId = request.PostId,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error),
            error => new Forbid<PermissionDeniedError>(error)
        );
    }
}