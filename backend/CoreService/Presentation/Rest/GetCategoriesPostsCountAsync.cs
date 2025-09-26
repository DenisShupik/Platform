using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Ok<Dictionary<CategoryId, ulong>>> GetCategoriesPostsCountAsync(
        GetCategoriesPostsCountRequest request,
        [FromServices] GetCategoriesPostsCountQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPostsCountQuery
        {
            CategoryIds = request.CategoryIds
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}