using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    private static IEndpointRouteBuilder ActivityApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/activities")
            .WithTags(nameof(ActivityApi))
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetActivitiesPagedAsync);

        return app;
    }

    private static IEndpointRouteBuilder CategoryApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/categories")
            .WithTags(nameof(CategoryApi))
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetCategoriesPagedAsync);
        api.MapGet("{categoryId}", GetCategoryAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{categoryIds}/posts/count", GetCategoriesPostsCountAsync);
        api.MapGet("{categoryIds}/posts/latest", GetCategoriesPostsLatestAsync);
        api.MapGet("{categoryIds}/threads/count", GetCategoriesThreadsCountAsync);
        api.MapGet("{categoryId}/threads", GetCategoryThreadsPagedAsync);
        api.MapPost(string.Empty, CreateCategoryAsync).RequireAuthorization();

        return app;
    }

    private static IEndpointRouteBuilder ForumApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/forums")
            .WithTags(nameof(ForumApi))
            .AddFluentValidationAutoValidation();

        api.MapGet("/count", GetForumsCountAsync);
        api.MapGet(string.Empty, GetForumsPagedAsync);
        api.MapGet("{forumId}", GetForumAsync).AllowAnonymous().RequireAuthorization();
        api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync);
        api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();

        return app;
    }

    private static IEndpointRouteBuilder PostApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/posts")
            .WithTags(nameof(PostApi))
            .AddFluentValidationAutoValidation();

        api.MapGet("{postId}/order", GetPostIndexAsync).AllowAnonymous().RequireAuthorization();
        api.MapPatch("{postId}", UpdatePostAsync).RequireAuthorization();

        return app;
    }

    private static IEndpointRouteBuilder ThreadApi(this IEndpointRouteBuilder app)
    {
        var api = app
            .MapGroup("api/threads")
            .WithTags(nameof(ThreadApi))
            .AddFluentValidationAutoValidation();

        api.MapGet(string.Empty, GetThreadsPagedAsync);
        api.MapGet("count", GetThreadsCountAsync);
        api.MapGet("{threadId}", GetThreadAsync);
        api.MapGet("{threadId}/posts", GetThreadPostsPagedAsync);
        api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync);
        api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync);
        api.MapPost(string.Empty, CreateThreadAsync).RequireAuthorization();
        api.MapPost("{threadId}/posts", CreatePostAsync).RequireAuthorization();

        return app;
    }

    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app
            .ActivityApi()
            .CategoryApi()
            .ForumApi()
            .PostApi()
            .ThreadApi();

        return app;
    }
}