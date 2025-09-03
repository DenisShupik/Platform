using System.Security.Claims;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Abstractions;
using SharedKernel.Presentation.Extensions;
using UserService.Domain.ValueObjects;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ThreadApi
{
    public sealed class GetThreadsPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteriaList<GetThreadsPagedQuery.SortType> Sort =
            [
                new()
                {
                    Field = GetThreadsPagedQuery.SortType.ThreadId,
                    Order = SortOrderType.Ascending
                }
            ];
        }

        [FromQuery] public UserId? CreatedBy { get; set; }
        [FromQuery] public ThreadStatus? Status { get; set; }
        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetThreadsPagedQuery.SortType> Sort { get; set; } =
            Defaults.Sort;
    }


    private static async Task<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>
        GetThreadsPagedAsync(
            ClaimsPrincipal claimsPrincipal,
            [AsParameters] GetThreadsPagedRequest request,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserIdOrNull();
        var query = new GetThreadsPagedQuery
        {
            CreatedBy = request.CreatedBy,
            Status = request.Status,
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            QueriedBy = userId
        };

        var result = await messageBus.InvokeAsync<GetThreadsQueryResult<ThreadDto>>(query, cancellationToken);

        return result.Match<Results<Ok<List<ThreadDto>>, Forbid<NotAdminError>, Forbid<NotOwnerError>>>(
            threads => TypedResults.Ok(threads),
            notAdmin => new Forbid<NotAdminError>(notAdmin),
            notOwner => new Forbid<NotOwnerError>(notOwner)
        );
    }
}