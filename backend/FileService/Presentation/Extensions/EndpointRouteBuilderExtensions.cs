using FileService.Presentation.APIs;

namespace FileService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapAvatarApi()
            ;

        return app;
    }
}