using NotificationService.Presentation.Apis;

namespace NotificationService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapSubscriptionApi()
            ;

        return app;
    }
}