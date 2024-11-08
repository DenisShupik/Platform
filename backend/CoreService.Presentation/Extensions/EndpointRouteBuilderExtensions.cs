using CoreService.Presentation.Apis;

namespace CoreService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapForumApi()
            .MapCategoryApi()
            .MapThreadApi()
            .MapPostApi()
            ;

        return app;
    }
}