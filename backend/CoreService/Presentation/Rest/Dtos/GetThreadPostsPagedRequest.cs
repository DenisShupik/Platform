using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

public sealed class GetThreadPostsPagedRequest
{
    private static class Defaults
    {
        public static readonly SortCriteria<GetThreadPostsPagedQuery.SortType> Sort =
            new()
            {
                Field = GetThreadPostsPagedQuery.SortType.Index,
                Order = SortOrderType.Ascending
            };
    }

    [FromRoute] public ThreadId ThreadId { get; set; }
    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;
    [FromQuery] public SortCriteria<GetThreadPostsPagedQuery.SortType> Sort { get; set; } = Defaults.Sort;
}