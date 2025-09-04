using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;
using UserService.Application.UseCases;

namespace UserService.Presentation.Rest.Dtos;

public sealed class GetUsersPagedRequest
{
    private static class Defaults
    {
        public static readonly SortCriteria<GetUsersPagedQuery.SortType> Sort =
            new()
            {
                Field = GetUsersPagedQuery.SortType.UserId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;
    [FromQuery] public SortCriteria<GetUsersPagedQuery.SortType> Sort { get; set; } = Defaults.Sort;
}