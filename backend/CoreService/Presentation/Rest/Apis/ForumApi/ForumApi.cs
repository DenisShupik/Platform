using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CoreService.Presentation.Rest.Apis;

public static partial class ForumApi
{
    public static IEndpointRouteBuilder MapForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetForumsCountAsync);
        api.MapGet(string.Empty, GetForumsPagedAsync);
        api.MapGet("{forumId}", GetForumAsync);
        api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync);
        api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();

        return app;
    }
}