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

    private static IEndpointRouteBuilder WatchedThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/watched-threads")
            .WithTags(nameof(WatchedThreadApi));

        api.MapGet("/latest-events", GetWatchedThreadLatestEventPagedAsync);

        return app;
    }

    private static IEndpointRouteBuilder SubscriptionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads/{threadId}/subscriptions")
            .WithTags(nameof(SubscriptionApi));

        api.MapGet("/status", GetThreadSubscriptionStatusAsync);
        api.MapPost(string.Empty, CreateThreadSubscriptionAsync);
        api.MapDelete(string.Empty, DeleteThreadSubscriptionAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        return app
            .SubscriptionApi()
            .InternalNotificationApi()
            .WatchedThreadApi();
    }
}