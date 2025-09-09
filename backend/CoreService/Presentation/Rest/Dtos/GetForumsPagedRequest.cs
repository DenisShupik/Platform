using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Generator;
using SharedKernel.Presentation.ValueObjects;
using UserService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

[GenerateBind]
public sealed partial class GetForumsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetForumsPagedQuery.SortType> Sort =
            new()
            {
                Field = GetForumsPagedQuery.SortType.ForumId,
                Order = SortOrderType.Ascending
            };
    }

    [FromQuery] public required ForumTitle? Title { get; init; }
    [FromQuery] public required UserId? CreatedBy { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetForumsPagedQuery.SortType> Sort { get; init; }
}