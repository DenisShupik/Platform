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
    private static async Task<Ok<Dictionary<CategoryId, ulong>>> GetCategoriesThreadsCountAsync(
        ClaimsPrincipal  claimsPrincipal,
        GetCategoriesThreadsCountRequest request,
        [FromServices] GetCategoriesThreadsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var queriedBy = claimsPrincipal.GetUserIdOrNull();
        var query = new GetCategoriesThreadsCountQuery
        {
            CategoryIds = request.CategoryIds,
            IncludeDraft = request.IncludeDraft,
            QueriedBy = queriedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}