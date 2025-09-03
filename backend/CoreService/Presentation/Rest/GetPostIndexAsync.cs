using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>> GetPostIndexAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromRoute] PostId postId,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var command = new GetPostIndexQuery
        {
            PostId = postId
        };

        var result = await messageBus.InvokeAsync<GetPostIndexQueryResult>(command, cancellationToken);

        return result.Match<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>>(
            order => TypedResults.Ok(order),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}