using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Generator;
using SharedKernel.Presentation.ValueObjects;
using UserService.Application.UseCases;

namespace UserService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetUsersPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetUsersPagedQuery.SortType> Sort =
            new()
            {
                Field = GetUsersPagedQuery.SortType.UserId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetUsersPagedQuery.SortType> Sort { get; init; }
}