using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Errors;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<ForumDto>,
    Unauthorized<ClaimNotFoundError>,
    NotFound<ForumNotFoundError>
>;

public static partial class Api
{
    /// <summary>
    /// Получить форум
    /// </summary>
    public static async Task<Response> GetForumAsync(
        GetForumRequest request,
        [FromServices] GetForumQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumQuery<ForumDto>
        {
            ForumId = request.ForumId,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error)
        );
    }
}