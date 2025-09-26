using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Presentation.Generator;
using Shared.Presentation.ValueObjects;
using UserService.Application.UseCases;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetUsersPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetUsersPagedQuerySortType> Sort =
            new()
            {
                Field = GetUsersPagedQuerySortType.UserId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetUsersPagedQuerySortType> Sort { get; init; }
}