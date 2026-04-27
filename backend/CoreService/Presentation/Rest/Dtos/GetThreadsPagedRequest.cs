using CoreService.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator;
using Shared.Presentation.Generator.Attributes;
using Shared.Presentation.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Optional)]
public sealed partial class GetThreadsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetThreadsPagedQuerySortType> Sort =
            new()
            {
                Field = GetThreadsPagedQuerySortType.ThreadId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required UserId? CreatedBy { get; init; }
    [FromQuery] public required ThreadState? Status { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetThreadsPagedQuerySortType> Sort { get; init; }
}