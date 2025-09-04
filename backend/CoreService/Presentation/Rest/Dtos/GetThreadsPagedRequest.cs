using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.ValueObjects;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

public sealed class GetThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly SortCriteria<GetThreadsPagedQuery.SortType> Sort =
            new()
            {
                Field = GetThreadsPagedQuery.SortType.ThreadId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public UserId? CreatedBy { get; set; }
    [FromQuery] public ThreadStatus? Status { get; set; }
    [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
    [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;
    [FromQuery] public SortCriteria<GetThreadsPagedQuery.SortType> Sort { get; set; } = Defaults.Sort;
}