using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<ForumId>,
    BadRequest<PolicyDowngradeError>
>;

public static partial class Api
{
    private static async Task<Response> CreateForumAsync(
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
            ReadPolicyValue = body.ReadPolicyValue,
            CategoryCreatePolicyValue = body.CategoryCreatePolicyValue,
            ThreadCreatePolicyValue = body.ThreadCreatePolicyValue,
            PostCreatePolicyValue = body.PostCreatePolicyValue,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match<Response>(
            categoryId => TypedResults.Ok(categoryId),
            error => TypedResults.BadRequest(error)
        );
    }
}