using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Presentation.Rest;

using Response = Results<
    Ok<CategoryDto>,
    NotFound<CategoryNotFoundError>
>;

public static partial class Api
{
    /// <summary>
    /// Получить раздел
    /// </summary>
    public static async Task<Response> GetCategoryAsync(
        GetCategoryRequest request,
        [FromServices] GetCategoryQueryHandler<CategoryDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryQuery<CategoryDto>
        {
            CategoryId = request.CategoryId,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Response>(
            categoryDto => TypedResults.Ok(categoryDto),
            error => TypedResults.NotFound(error)
        );
    }
}