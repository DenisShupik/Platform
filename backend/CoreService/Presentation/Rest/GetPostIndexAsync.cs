using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>> GetPostIndexAsync(
        ClaimsPrincipal claimsPrincipal,
        GetPostIndexRequest request,
        [FromServices] GetPostIndexQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var command = new GetPostIndexQuery
        {
            PostId = request.PostId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Results<Ok<PostIndex>, NotFound<PostNotFoundError>>>(
            order => TypedResults.Ok(order),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}