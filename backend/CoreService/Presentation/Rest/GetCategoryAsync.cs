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
    NotFound<CategoryNotFoundError>,
    Forbid<PolicyViolationError>,
    Forbid<ReadPolicyRestrictedError>
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
            error => TypedResults.NotFound(error),
            error => new Forbid<PolicyViolationError>(error),
            error => new Forbid<ReadPolicyRestrictedError>(error)
        );
    }
}