namespace UserService.Presentation.Rest;

public static partial class Api
{
    private static IEndpointRouteBuilder UserApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/users")
            .WithTags(nameof(UserApi));

        api.MapGet(string.Empty, GetUsersPagedAsync);
        api.MapGet("{userId}", GetUserAsync);
        api.MapGet("batch/{userIds}", GetUsersBulkAsync);

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.UserApi();

        return app;
    }
}