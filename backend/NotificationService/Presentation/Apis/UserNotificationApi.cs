using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using SharedKernel.Presentation.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using Wolverine;

namespace NotificationService.Presentation.Apis;

public static class UserNotificationApi
{
    public static IEndpointRouteBuilder MapUserNotificationApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/notifications")
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetUserNotificationCountAsync);

        return app;
    }

    private static async Task<Ok<int>> GetUserNotificationCountAsync(
        ClaimsPrincipal claimsPrincipal,
        [FromQuery] bool? isDelivered,
        [FromQuery] ChannelType? channel,
        [FromServices] IMessageBus messageBus,
        CancellationToken cancellationToken
    )
    {
        var userId = claimsPrincipal.GetUserId();
        var command = new GetUserNotificationCountQuery
        {
            UserId = userId,
            IsDelivered = isDelivered,
            Channel = channel
        };
        var result = await messageBus.InvokeAsync<int>(command, cancellationToken);

        return TypedResults.Ok(result);
    }
}