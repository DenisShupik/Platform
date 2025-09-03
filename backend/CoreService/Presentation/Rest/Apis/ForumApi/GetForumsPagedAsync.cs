using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using UserService.Domain.ValueObjects;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ForumApi
{
    public sealed class GetForumsPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteria<GetForumsPagedQuery.GetForumsPagedQuerySortType> Sort =
                new()
                {
                    Field = GetForumsPagedQuery.GetForumsPagedQuerySortType.ForumId,
                    Order = SortOrderType.Ascending
                };
        }

        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteria<GetForumsPagedQuery.GetForumsPagedQuerySortType> Sort { get; set; } = Defaults.Sort;

        [FromQuery] public ForumTitle? Title { get; set; }
        [FromQuery] public UserId? CreatedBy { get; set; }
    }
    
    private static async Task<Ok<GetForumsPagedQueryResult>> GetForumsPagedAsync(
        [AsParameters] GetForumsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetForumsPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            Title = request.Title,
            CreatedBy = request.CreatedBy
        };

        var result = await messageBus.InvokeAsync<GetForumsPagedQueryResult>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}