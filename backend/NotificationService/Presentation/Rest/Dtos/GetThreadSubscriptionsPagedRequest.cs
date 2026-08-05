using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.ValueObjects;
using Shared.Presentation.Generator.Attributes;
using Shared.Presentation.ValueObjects;

namespace NotificationService.Presentation.Rest.Dtos;

[GenerateBind(AuthorizeMode.Required)]
public sealed partial class GetThreadSubscriptionsPagedRequest
{
    private static class Defaults
    {
        public static readonly PaginationOffset Offset = PaginationOffset.Default;
        public static readonly PaginationLimitMin10Max100 Limit = PaginationLimitMin10Max100.Default100;

        public static readonly SortCriteria<GetThreadSubscriptionsPagedQuerySortType> Sort =
            new()
            {
                Field = GetThreadSubscriptionsPagedQuerySortType.ThreadId,
                Order = SortOrderType.Ascending
            };
    }

    [FromRoute] public required UserId UserId { get; init; }
    [FromQuery] public required PaginationOffset Offset { get; init; }
    [FromQuery] public required PaginationLimitMin10Max100 Limit { get; init; }
    [FromQuery] public required SortCriteria<GetThreadSubscriptionsPagedQuerySortType> Sort { get; init; }
}
