namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static IEndpointRouteBuilder InternalNotificationApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/notifications")
            .WithTags(nameof(InternalNotificationApi));

        api.MapGet("/count", GetInternalNotificationCountAsync)
            .WithSummary("Get the current user's notification count");
        api.MapGet(string.Empty, GetInternalNotificationsPagedAsync)
            .WithSummary("Get the current user's notifications");
        api.MapPut("/{notifiableEventId}/mark-read", MarkInternalNotificationAsReadAsync)
            .WithSummary("Mark a notification as read");
        api.MapDelete("/{notifiableEventId}", DeleteInternalNotificationAsync)
            .WithSummary("Delete a notification");
        return app;
    }

    private static IEndpointRouteBuilder UserSubscriptionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/users/{userId}/subscriptions")
            .WithTags(nameof(UserSubscriptionApi));

        api.MapGet(string.Empty, GetThreadSubscriptionsPagedAsync)
            .WithSummary("Get a user's thread subscriptions");
        api.MapGet("/latest-events", GetThreadSubscriptionLatestEventsPagedAsync)
            .WithSummary("Get the latest events for a user's thread subscriptions");
        api.MapGet("/{threadId}/status", GetThreadSubscriptionStatusAsync)
            .WithSummary("Get a thread subscription status");
        api.MapPost("/{threadId}", CreateThreadSubscriptionAsync)
            .WithSummary("Create a thread subscription");
        api.MapDelete("/{threadId}", DeleteThreadSubscriptionAsync)
            .WithSummary("Delete a thread subscription");

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        return app
            .InternalNotificationApi()
            .UserSubscriptionApi();
    }
}
