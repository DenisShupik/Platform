using NotificationService.Application.Dtos;
using NotificationService.Application.UseCases;
using NotificationService.Presentation.Rest.Dtos;
using Shared.Presentation.Extensions;

namespace NotificationService.Presentation.Rest;

public static partial class Api
{
    extension(IEndpointRouteBuilder app)
    {
        private IEndpointRouteBuilder InternalNotificationApi()
        {
            var api = app
                .MapGroup("api/me/notifications")
                .WithTags(nameof(InternalNotificationApi));

            api.MapGet<GetInternalNotificationCountRequest, GetInternalNotificationCountQueryHandler>("count");
            api.MapGet<GetInternalNotificationsPagedRequest, GetInternalNotificationsPagedQueryHandler>(string.Empty);
            api.MapPut<MarkInternalNotificationAsReadRequest, MarkInternalNotificationAsReadCommandHandler>("{notifiableEventId}/mark-read");
            api.MapDelete<DeleteInternalNotificationRequest, DeleteInternalNotificationCommandHandler>("{notifiableEventId}");
            return app;
        }

        private IEndpointRouteBuilder UserSubscriptionApi()
        {
            var api = app
                .MapGroup("api/users/{userId}/subscriptions")
                .WithTags(nameof(UserSubscriptionApi));

            api.MapGet<GetThreadSubscriptionsPagedRequest, GetThreadSubscriptionsPagedQueryHandler>(string.Empty);
            api.MapGet<GetThreadSubscriptionLatestEventsPagedRequest, GetThreadSubscriptionLatestEventsPagedQueryHandler<ThreadSubscriptionLatestEventDto>>("latest-events");
            api.MapGet<GetThreadSubscriptionStatusRequest, GetThreadSubscriptionStatusQueryHandler>("{threadId}/status");
            api.MapPost<CreateThreadSubscriptionRequest, CreateThreadSubscriptionCommandHandler>("{threadId}");
            api.MapDelete<DeleteThreadSubscriptionRequest, DeleteThreadSubscriptionCommandHandler>("{threadId}");

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            return app
                .InternalNotificationApi()
                .UserSubscriptionApi();
        }
    }
}
