using System.Security.Claims;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<Dictionary<PolicyType, bool>>,
    NotFound<CategoryNotFoundError>
>;

public static partial class Api
{
    private static async Task<Response> GetCategoryPermissionsAsync(
        ClaimsPrincipal claimsPrincipal,
        GetCategoryPermissionsRequest request,
        [FromServices] GetCategoryPermissionsQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();

        var query = new GetCategoryPermissionsQuery
        {
            CategoryId = request.CategoryId,
            QueriedBy = queriedBy,
            EvaluatedAt = DateTime.UtcNow
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            value => TypedResults.Ok(value),
            error => TypedResults.NotFound(error)
        );
    }
}