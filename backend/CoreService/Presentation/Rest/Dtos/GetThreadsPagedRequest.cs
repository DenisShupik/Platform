using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Generator;
using SharedKernel.Presentation.ValueObjects;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetThreadsPagedQuery.SortType> Sort =
            new()
            {
                Field = GetThreadsPagedQuery.SortType.ThreadId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required UserId? CreatedBy { get; init; }
    [FromQuery] public required ThreadStatus? Status { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetThreadsPagedQuery.SortType> Sort { get; init; }
}