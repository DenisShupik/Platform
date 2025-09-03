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

namespace CoreService.Presentation.Rest.Apis;

public static partial class ThreadApi
{
    public sealed class GetThreadPostsPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteriaList<GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType> Sort =
            [
                new()
                {
                    Field = GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType.Index,
                    Order = SortOrderType.Ascending
                }
            ];
        }

        [FromRoute] public ThreadId ThreadId { get; set; }
        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetThreadPostsPagedQuery.GetThreadPostsPagedQuerySortType> Sort { get; set; } =
            Defaults.Sort;
    }

    private static async Task<Ok<IReadOnlyList<PostDto>>> GetThreadPostsPagedAsync(
        [AsParameters] GetThreadPostsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetThreadPostsPagedQuery
        {
            ThreadId = request.ThreadId,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<PostDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}