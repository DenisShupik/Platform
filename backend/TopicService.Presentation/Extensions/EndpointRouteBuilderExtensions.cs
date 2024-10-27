using TopicService.Presentation.Apis;

namespace TopicService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapSectionApi()
            .MapTopicApi();

        return app;
    }
}