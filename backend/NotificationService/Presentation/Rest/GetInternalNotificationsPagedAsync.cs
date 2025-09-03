using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Enums;
using SharedKernel.Application.ValueObjects;
using SharedKernel.Presentation.Extensions;
using SharedKernel.Presentation.ValueObjects;
using Wolverine;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    public sealed class GetInternalNotificationsPagedRequest
    {
        private static class Defaults
        {
            public static readonly
                SortCriteriaList<GetInternalNotificationsPagedQuery.GetInternalNotificationQuerySortType> Sort =
                [
                    new()
                    {
                        Field = GetInternalNotificationsPagedQuery.GetInternalNotificationQuerySortType.OccurredAt,
                        Order = SortOrderType.Ascending
                    }
                ];
        }

        [FromQuery] public PaginationOffset Offset { get; set; } = PaginationOffset.Default;
        [FromQuery] public PaginationLimitMin10Max100 Limit { get; set; } = PaginationLimitMin10Max100.Default100;

        [FromQuery]
        public SortCriteriaList<GetInternalNotificationsPagedQuery.GetInternalNotificationQuerySortType> Sort
        {
            get;
            set;
        } = Defaults.Sort;

        [FromQuery] public bool? IsDelivered { get; set; }
    }

    private static async Task<Ok<InternalNotificationsPagedDto>> GetInternalNotificationsPagedAsync(
        ClaimsPrincipal claimsPrincipal,
        [AsParameters] GetInternalNotificationsPagedRequest request,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationsPagedQuery
        {
            Offset = request.Offset,
            Limit = PaginationLimit.From(request.Limit.Value),
            Sort = request.Sort,
            UserId = userId,
            IsDelivered = request.IsDelivered
        };

        var result = await messageBus.InvokeAsync<InternalNotificationsPagedDto>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}