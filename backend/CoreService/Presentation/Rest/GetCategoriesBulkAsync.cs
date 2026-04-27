using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Presentation.Rest;

using Response =
    Ok<Dictionary<CategoryId, Result<CategoryDto, CategoryNotFoundError>>>;

public static partial class Api
{
    /// <summary>
    /// Получить разделы по списку идентификаторов
    /// </summary>
    public static async Task<Response> GetCategoriesBulkAsync(
        GetCategoriesBulkRequest request,
        [FromServices] GetCategoriesBulkQueryHandler<CategoryDto> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesBulkQuery<CategoryDto>
        {
            CategoryIds = request.CategoryIds,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}