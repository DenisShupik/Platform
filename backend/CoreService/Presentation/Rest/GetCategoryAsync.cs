using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Abstractions;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<CategoryDto>,
    Forbid<ForumAccessLevelError>,
    Forbid<CategoryAccessLevelError>,
    Forbid<ForumAccessRestrictedError>,
    Forbid<CategoryAccessRestrictedError>,
    NotFound<CategoryNotFoundError>
>;

public static partial class Api
{
    private static async Task<Response> GetCategoryAsync(
        ClaimsPrincipal claimsPrincipal,
        GetCategoryRequest request,
        [FromServices] GetCategoryQueryHandler<CategoryDto> handler,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetCategoryQuery<CategoryDto>
        {
            CategoryId = request.CategoryId,
            QueriedBy = userId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            categoryDto => TypedResults.Ok(categoryDto),
            forumAccessLevelError => new Forbid<ForumAccessLevelError>(forumAccessLevelError),
            categoryAccessLevelError => new Forbid<CategoryAccessLevelError>(categoryAccessLevelError),
            forumAccessRestrictedError => new Forbid<ForumAccessRestrictedError>(forumAccessRestrictedError),
            categoryAccessRestrictedError => new Forbid<CategoryAccessRestrictedError>(categoryAccessRestrictedError),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}