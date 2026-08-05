namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static IEndpointRouteBuilder InternalNotificationApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/notifications")
            .WithTags(nameof(InternalNotificationApi));

        api.MapGet("/count", GetInternalNotificationCountAsync);
        api.MapGet(string.Empty, GetInternalNotificationsPagedAsync);
        api.MapPut("/{notifiableEventId}/mark-read", MarkInternalNotificationAsReadAsync);
        api.MapDelete("/{notifiableEventId}", DeleteInternalNotificationAsync);
        return app;
    }

    private static IEndpointRouteBuilder UserSubscriptionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/users/{userId}/subscriptions")
            .WithTags(nameof(UserSubscriptionApi));

        api.MapGet(string.Empty, GetThreadSubscriptionsPagedAsync);
        api.MapGet("/latest-events", GetThreadSubscriptionLatestEventsPagedAsync);
        api.MapGet("/{threadId}/status", GetThreadSubscriptionStatusAsync);
        api.MapPost("/{threadId}", CreateThreadSubscriptionAsync);
        api.MapDelete("/{threadId}", DeleteThreadSubscriptionAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        return app
            .InternalNotificationApi()
            .UserSubscriptionApi();
    }
}
