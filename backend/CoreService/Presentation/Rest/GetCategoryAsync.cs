using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static async Task<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>> GetCategoryAsync(
        GetCategoryRequest request,
        [FromServices] GetCategoryQueryHandler<CategoryDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryQuery<CategoryDto>
        {
            CategoryId = request.CategoryId
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<CategoryDto>, NotFound<CategoryNotFoundError>>>(
            categoryDto => TypedResults.Ok(categoryDto),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}