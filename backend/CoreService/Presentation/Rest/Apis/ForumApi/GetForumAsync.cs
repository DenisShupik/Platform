using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using Wolverine;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ForumApi
{
    private static async Task<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>> GetForumAsync(
        [FromRoute] ForumId forumId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumQuery
        {
            ForumId = forumId
        };

        var result = await messageBus.InvokeAsync<OneOf<ForumDto, ForumNotFoundError>>(query, cancellationToken);

        return result.Match<Results<Ok<ForumDto>, NotFound<ForumNotFoundError>>>(
            forumDto => TypedResults.Ok(forumDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}