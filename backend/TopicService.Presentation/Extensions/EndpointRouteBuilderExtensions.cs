using TopicService.Presentation.Apis;

namespace TopicService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapSectionApi()
            .MapCategoryApi()
            .MapTopicApi()
            .MapPostApi()
            ;

        return app;
    }
}