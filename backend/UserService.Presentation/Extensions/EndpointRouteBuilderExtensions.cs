using UserService.Presentation.Apis;

namespace UserService.Presentation.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .MapUserApi()
            ;

        return app;
    }
}