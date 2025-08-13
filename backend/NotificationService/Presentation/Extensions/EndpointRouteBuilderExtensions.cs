using NotificationService.Presentation.Apis;

namespace NotificationService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        return app
            .MapSubscriptionApi()
            .MapInternalNotificationApi();
    }
}