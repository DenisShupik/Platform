using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    private static IEndpointRouteBuilder InternalNotificationApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/me/notifications")
            .WithTags(nameof(InternalNotificationApi))
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetInternalNotificationCountAsync);
        api.MapGet(string.Empty, GetInternalNotificationsPagedAsync);
        api.MapPut("/{notifiableEventId}/mark-read", MarkInternalNotificationAsReadAsync);
        api.MapDelete("/{notifiableEventId}", DeleteInternalNotificationAsync);
        return app;
    }

    private static IEndpointRouteBuilder SubscriptionApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/thread/{threadId}/subscriptions")
            .WithTags(nameof(SubscriptionApi))
            .RequireAuthorization()
            .AddFluentValidationAutoValidation();

        api.MapGet("/status", GetThreadSubscriptionStatusAsync);
        api.MapPost(string.Empty, CreateThreadSubscriptionAsync);
        api.MapDelete(string.Empty, DeleteThreadSubscriptionAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        return app
            .SubscriptionApi()
            .InternalNotificationApi();
    }
}