using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<ForumId>> CreateForumAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateForumRequestBody body,
        [FromServices] CreateForumCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateForumCommand
        {
            Title = body.Title,
            AccessLevel = body.AccessLevel,
            CreatedBy = userId
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}