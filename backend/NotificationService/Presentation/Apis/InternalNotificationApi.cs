using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using SharedKernel.Application.Abstractions;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Apis;

public static class InternalNotificationApi
{
    public static IEndpointRouteBuilder MapInternalNotificationApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/notifications")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetInternalNotificationCountAsync);
        api.MapGet(string.Empty, GetInternalNotificationsPagedAsync);
        api.MapPut("/{notifiableEventId}/mark-read", MarkInternalNotificationAsReadAsync);
        api.MapDelete("/{notifiableEventId}", DeleteInternalNotificationAsync);
        return app;
    }

    private static async Task<Ok<int>> GetInternalNotificationCountAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromQuery] bool? isDelivered,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationCountQuery
        {
            UserId = userId,
            IsDelivered = isDelivered
        };
        var result = await messageBus.InvokeAsync<int>(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<InternalNotificationsPagedDto>> GetInternalNotificationsPagedAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromQuery] int? offset,
        [FromQuery] int? limit,
        [FromQuery] SortCriteriaList<GetInternalNotificationsPagedQuery.GetInternalNotificationQuerySortType>? sort,
        [FromQuery] bool? isDelivered,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetInternalNotificationsPagedQuery
        {
            Offset = offset ?? 0,
            Limit = limit ?? 50,
            Sort = sort,
            UserId = userId,
            IsDelivered = isDelivered
        };
        var result = await messageBus.InvokeAsync<InternalNotificationsPagedDto>(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Results<NoContent, NotFound<NotificationNotFoundError>>>
        MarkInternalNotificationAsReadAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] NotifiableEventId notifiableEventId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new MarkInternalNotificationAsReadCommand
        {
            UserId = userId,
            NotifiableEventId = notifiableEventId
        };
        var result =
            await messageBus.InvokeAsync<MarkInternalNotificationAsReadCommandResult>(command, cancellationToken);

        return result.Match<Results<NoContent, NotFound<NotificationNotFoundError>>>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }

    private static async Task<Results<NoContent, NotFound<NotificationNotFoundError>>>
        DeleteInternalNotificationAsync(
            ClaimsPrincipal claimsPrincipal,
            [FromRoute] NotifiableEventId notifiableEventId,
            [FromServices] IMessageBus messageBus,
            CancellationToken cancellationToken
        )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new DeleteInternalNotificationCommand
        {
            UserId = userId,
            NotifiableEventId = notifiableEventId
        };
        var result =
            await messageBus.InvokeAsync<OneOf<Success, NotificationNotFoundError>>(command, cancellationToken);

        return result.Match<Results<NoContent, NotFound<NotificationNotFoundError>>>(
            _ => TypedResults.NoContent(),
            error => TypedResults.NotFound(error)
        );
    }
}