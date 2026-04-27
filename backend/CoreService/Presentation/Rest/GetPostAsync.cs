using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<PostDto>,
    NotFound<PostNotFoundError>,
    Forbid<PermissionDeniedError>
>;

public static partial class Api
{
    /// <summary>
    /// Получить сообщение
    /// </summary>
    public static async Task<Response> GetPostAsync(
        GetPostRequest request,
        [FromServices] GetPostQueryHandler<PostDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetPostQuery<PostDto>
        {
            PostId = request.PostId,
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