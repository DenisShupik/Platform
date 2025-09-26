using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>> GetForumAsync(
        GetForumRequest request,
        [FromServices] GetForumQueryHandler<ForumDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumQuery<ForumDto>
        {
            ForumId = request.ForumId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>>(
            forumDto => TypedResults.Ok(forumDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}