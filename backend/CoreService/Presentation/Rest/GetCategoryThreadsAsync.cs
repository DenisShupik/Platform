using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    public sealed class GetCategoryThreadsPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteria<GetCategoryThreadsQuery.SortType> Sort =
                new()
                {
                    Field = GetCategoryThreadsQuery.SortType.Activity,
                    Order = SortOrderType.Ascending
                };
        }

        [FromRoute] public CategoryId CategoryId { get; set; }
        [FromQuery] public bool IncludeDraft { get; set; } = false;
        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteria<GetCategoryThreadsQuery.SortType> Sort { get; set; } =
            Defaults.Sort;
    }

    private static async Task<Results<NotFound, Ok<IReadOnlyList<ThreadDto>>>> GetCategoryThreadsAsync(
        [AsParameters] GetCategoryThreadsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetCategoryThreadsQuery
        {
            CategoryId = request.CategoryId,
            IncludeDraft = request.IncludeDraft,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<ThreadDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}