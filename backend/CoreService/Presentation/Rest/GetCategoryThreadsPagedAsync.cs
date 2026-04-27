using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.ValueObjects;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    /// <summary>
    /// Получить постраничный список тем в разделе
    /// </summary>
    public static async Task<Results<Ok<IReadOnlyList<ThreadDto>>, NotFound<CategoryNotFoundError>>>
        GetCategoryThreadsPagedAsync(
            GetCategoryThreadsPagedRequest request,
            [FromServices] GetCategoryThreadsPagedQueryHandler<ThreadDto> handler,
            CancellationToken cancellationToken
        )
    {
        var query = new GetCategoryThreadsPagedQuery<ThreadDto>
        {
            CategoryId = request.CategoryId,
            State = request.State,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = request.RequestedBy
        };

        var result = await handler.HandleAsync(query, cancellationToken);

        return result.Match<Results<Ok<IReadOnlyList<ThreadDto>>, NotFound<CategoryNotFoundError>>>(
            value => TypedResults.Ok(value),
            notFound => TypedResults.NotFound(notFound)
        );
    }
}