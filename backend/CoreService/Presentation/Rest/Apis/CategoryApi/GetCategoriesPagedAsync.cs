using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

namespace CoreService.Presentation.Rest.Apis;

public static partial class CategoryApi
{
    public sealed class GetCategoriesPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteriaList<GetCategoriesPagedQuery.GetCategoriesPagedQuerySortType> Sort =
            [
                new()
                {
                    Field = GetCategoriesPagedQuery.GetCategoriesPagedQuerySortType.CategoryId,
                    Order = SortOrderType.Ascending
                }
            ];
        }

        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetCategoriesPagedQuery.GetCategoriesPagedQuerySortType> Sort { get; set; } =
            Defaults.Sort;

        [FromQuery] public IdSet<ForumId>? ForumIds { get; set; }
        [FromQuery] public CategoryTitle? Title { get; set; }
    }

    private static async Task<Ok<GetCategoriesPagedQueryResult>> GetCategoriesPagedAsync(
        [AsParameters] GetCategoriesPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoriesPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            ForumIds = request.ForumIds,
            Title = request.Title
        };

        var result = await messageBus.InvokeAsync<GetCategoriesPagedQueryResult>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}