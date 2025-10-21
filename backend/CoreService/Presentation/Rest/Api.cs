using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    extension(IEndpointRouteBuilder app)
    {
        private IEndpointRouteBuilder PortalApi()
        {
            var api = app
                .MapGroup("api/portal")
                .WithTags(nameof(PortalApi))
                .AddFluentValidationAutoValidation();

            api.MapGet(string.Empty, GetPortalAsync);
            api.MapGet("/permissions", GetPortalPermissionsAsync).AllowAnonymous().RequireAuthorization();

            return app;
        }

        private IEndpointRouteBuilder ForumApi()
        {
            var api = app
                .MapGroup("api/forums")
                .WithTags(nameof(ForumApi))
                .AddFluentValidationAutoValidation();

            api.MapGet(string.Empty, GetForumsPagedAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("/count", GetForumsCountAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{forumId}", GetForumAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{forumId}/permissions", GetForumPermissionsAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("/bulk/{forumIds}", GetForumsBulkAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync).AllowAnonymous()
                .RequireAuthorization();

            api.MapPost(string.Empty, CreateForumAsync).RequireAuthorization();


            return app;
        }

        private IEndpointRouteBuilder CategoryApi()
        {
            var api = app
                .MapGroup("api/categories")
                .WithTags(nameof(CategoryApi))
                .AllowAnonymous()
                .RequireAuthorization()
                .AddFluentValidationAutoValidation();

            api.MapGet(string.Empty, GetCategoriesPagedAsync);
            api.MapGet("{categoryId}", GetCategoryAsync);
            api.MapGet("{categoryId}/permissions", GetCategoryPermissionsAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{categoryIds}/posts/count", GetCategoriesPostsCountAsync);
            api.MapGet("{categoryIds}/posts/latest", GetCategoriesPostsLatestAsync);
            api.MapGet("{categoryIds}/threads/count", GetCategoriesThreadsCountAsync);
            api.MapGet("{categoryId}/threads", GetCategoryThreadsPagedAsync);
            api.MapPost(string.Empty, CreateCategoryAsync);

            return app;
        }

        private IEndpointRouteBuilder ThreadApi()
        {
            var api = app
                .MapGroup("api/threads")
                .WithTags(nameof(ThreadApi))
                .AddFluentValidationAutoValidation();

            api.MapGet(string.Empty, GetThreadsPagedAsync);
            api.MapGet("count", GetThreadsCountAsync);
            api.MapGet("{threadId}", GetThreadAsync);
            api.MapGet("{threadId}/permissions", GetThreadPermissionsAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{threadId}/posts", GetThreadPostsPagedAsync);
            api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync).AllowAnonymous().RequireAuthorization();
            api.MapPost(string.Empty, CreateThreadAsync).AllowAnonymous().RequireAuthorization();
            api.MapPost("{threadId}/posts", CreatePostAsync).AllowAnonymous().RequireAuthorization();

            return app;
        }

        private IEndpointRouteBuilder PostApi()
        {
            var api = app
                .MapGroup("api/posts")
                .WithTags(nameof(PostApi))
                .AddFluentValidationAutoValidation();

            api.MapGet("{postId}", GetPostAsync).AllowAnonymous().RequireAuthorization();
            api.MapGet("{postId}/order", GetPostIndexAsync).AllowAnonymous().RequireAuthorization();
            api.MapPatch("{postId}", UpdatePostAsync).RequireAuthorization();

            return app;
        }

        private IEndpointRouteBuilder PolicyApi()
        {
            var api = app
                .MapGroup("api/policies")
                .WithTags(nameof(PolicyApi))
                .AddFluentValidationAutoValidation();

            api.MapGet("/bulk/{policyIds}", GetPoliciesBulkAsync);

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app
                .PortalApi()
                .ForumApi()
                .ThreadApi()
                .CategoryApi()
                .PostApi()
                .PolicyApi()
                ;

            return app;
        }
    }
}