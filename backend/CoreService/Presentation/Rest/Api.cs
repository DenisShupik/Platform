using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Rest;

public static partial class Api
{
    extension(IEndpointRouteBuilder app)
    {
        private IEndpointRouteBuilder ForumApi()
        {
            var api = app
                .MapGroup("api/forums")
                .WithTags(nameof(ForumApi))
                .WithAutoNames();

            api.MapGet(string.Empty, GetForumsPagedAsync);
            api.MapGet("/count", GetForumsCountAsync);
            api.MapGet("{forumId}", GetForumAsync);
            api.MapGet("/bulk/{forumIds}", GetForumsBulkAsync);
            api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync);
            api.MapPost(string.Empty, CreateForumAsync);

            return app;
        }

        private IEndpointRouteBuilder CategoryApi()
        {
            var api = app
                .MapGroup("api/categories")
                .WithTags(nameof(CategoryApi))
                .WithAutoNames();

            api.MapGet(string.Empty, GetCategoriesPagedAsync);
            api.MapGet("{categoryId}", GetCategoryAsync);
            api.MapGet("/bulk/{categoryIds}", GetCategoriesBulkAsync);
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
                .WithAutoNames();

            api.MapGet(string.Empty, GetThreadsPagedAsync);
            api.MapGet("count", GetThreadsCountAsync);
            api.MapGet("{threadId}", GetThreadAsync);
            api.MapGet("/bulk/{threadIds}", GetThreadsBulkAsync);
            api.MapGet("{threadId}/posts", GetThreadPostsPagedAsync);
            api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync);
            api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync);
            api.MapPost("{threadId}/request-approval", RequestThreadApprovalAsync);
            api.MapPost("{threadId}/approve", ApproveThreadAsync);
            api.MapPost("{threadId}/reject", RejectThreadAsync);
            api.MapPost(string.Empty, CreateThreadAsync);
            api.MapPost("{threadId}/posts", CreatePostAsync);

            return app;
        }

        private IEndpointRouteBuilder PostApi()
        {
            var api = app
                .MapGroup("api/posts")
                .WithTags(nameof(PostApi))
                .WithAutoNames();

            api.MapGet("{postId}", GetPostAsync);
            api.MapGet("{postId}/order", GetPostIndexAsync);
            api.MapPatch("{postId}", UpdatePostAsync);
            api.MapDelete("{postId}", DeletePostAsync);
            return app;
        }

        private IEndpointRouteBuilder PostBookmarkApi()
        {
            var api = app
                .MapGroup("api/posts/bookmarks")
                .WithTags(nameof(PostBookmarkApi))
                .WithAutoNames();

            api.MapGet("/bulk/{postIds}", GetBookmarkedPostIdsAsync);
            api.MapPost("/{postId}", CreatePostBookmarkAsync);
            api.MapDelete("/{postId}", DeletePostBookmarkAsync);

            return app;
        }

        private IEndpointRouteBuilder UserBookmarkApi()
        {
            var api = app
                .MapGroup("api/users/{userId}/bookmarks")
                .WithTags(nameof(UserBookmarkApi))
                .WithAutoNames();

            api.MapGet("/count", GetBookmarkedPostsCountAsync);
            api.MapGet(string.Empty, GetBookmarkedPostsPagedAsync);

            return app;
        }

        private IEndpointRouteBuilder SearchApi()
        {
            var api = app
                .MapGroup("api/search")
                .WithTags(nameof(SearchApi))
                .WithAutoNames();

            api.MapGet(string.Empty, SearchAsync);

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app
                .ForumApi()
                .CategoryApi()
                .ThreadApi()
                .PostApi()
                .PostBookmarkApi()
                .UserBookmarkApi()
                .SearchApi()
                ;

            return app;
        }
    }
}
