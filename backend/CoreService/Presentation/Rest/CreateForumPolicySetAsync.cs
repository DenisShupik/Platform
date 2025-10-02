using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Ok<ForumPolicySetId>;

public static partial class Api
{
    private static async Task<Response> CreateForumPolicySetAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromBody] CreateForumPolicySetRequestBody request,
        [FromServices] CreateForumPolicySetCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new CreateForumPolicySetCommand
        {
            Access = request.Access,
            CategoryCreate = request.CategoryCreate,
            ThreadCreate = request.ThreadCreate,
            PostCreate = request.PostCreate,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}