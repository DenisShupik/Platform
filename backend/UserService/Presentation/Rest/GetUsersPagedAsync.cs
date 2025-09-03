using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using Wolverine;
using PaginationLimitMin10Max100 = SharedKernel.Presentation.ValueObjects.PaginationLimitMin10Max100;

namespace UserService.Presentation.Rest;

public static partial class UserApi
{
    public sealed class GetUsersPagedRequest
    {
        private static class Defaults
        {
            public static readonly SortCriteriaList<GetUsersPagedQuery.GetUsersPagedQuerySortType> Sort =
            [
                new()
                {
                    Field = GetUsersPagedQuery.GetUsersPagedQuerySortType.UserId,
                    Order = SortOrderType.Ascending
                }
            ];
        }

        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetUsersPagedQuery.GetUsersPagedQuerySortType> Sort { get; set; } =
            Defaults.Sort;
    }

    private static async Task<Results<Ok<IReadOnlyList<UserDto>>, BadRequest<string>>> GetUsersPagedAsync(
        [AsParameters] GetUsersPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var query = new GetUsersPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort
        };

        var result = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return TypedResults.Ok(result);
    }
}