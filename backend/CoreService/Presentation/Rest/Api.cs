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

            api.MapGet(string.Empty, GetForumsPagedAsync).WithSummary("Get forums");
            api.MapGet("/count", GetForumsCountAsync).WithSummary("Get the forum count");
            api.MapGet("{forumId}", GetForumAsync).WithSummary("Get a forum");
            api.MapGet("/bulk/{forumIds}", GetForumsBulkAsync).WithSummary("Get forums by ID");
            api.MapGet("{forumIds}/categories/count", GetForumsCategoriesCountAsync)
                .WithSummary("Get category counts for forums");
            api.MapPost(string.Empty, CreateForumAsync).WithSummary("Create a forum");

            return app;
        }

        private IEndpointRouteBuilder CategoryApi()
        {
            var api = app
                .MapGroup("api/categories")
                .WithTags(nameof(CategoryApi))
                .WithAutoNames();

            api.MapGet(string.Empty, GetCategoriesPagedAsync).WithSummary("Get categories");
            api.MapGet("{categoryId}", GetCategoryAsync).WithSummary("Get a category");
            api.MapGet("/bulk/{categoryIds}", GetCategoriesBulkAsync).WithSummary("Get categories by ID");
            api.MapGet("{categoryIds}/posts/count", GetCategoriesPostsCountAsync)
                .WithSummary("Get post counts for categories");
            api.MapGet("{categoryIds}/posts/latest", GetCategoriesPostsLatestAsync)
                .WithSummary("Get the latest posts for categories");
            api.MapGet("{categoryIds}/threads/count", GetCategoriesThreadsCountAsync)
                .WithSummary("Get thread counts for categories");
            api.MapGet("{categoryId}/threads", GetCategoryThreadsPagedAsync)
                .WithSummary("Get threads in a category");
            api.MapPost(string.Empty, CreateCategoryAsync).WithSummary("Create a category");

            return app;
        }

        private IEndpointRouteBuilder ThreadApi()
        {
            var api = app
                .MapGroup("api/threads")
                .WithTags(nameof(ThreadApi))
                .WithAutoNames();

            api.MapGet(string.Empty, GetThreadsPagedAsync).WithSummary("Get threads");
            api.MapGet("count", GetThreadsCountAsync).WithSummary("Get the thread count");
            api.MapGet("{threadId}", GetThreadAsync).WithSummary("Get a thread");
            api.MapGet("/bulk/{threadIds}", GetThreadsBulkAsync).WithSummary("Get threads by ID");
            api.MapGet("{threadId}/posts", GetThreadPostsPagedAsync).WithSummary("Get posts in a thread");
            api.MapGet("{threadIds}/posts/count", GetThreadsPostsCountAsync)
                .WithSummary("Get post counts for threads");
            api.MapGet("{threadIds}/posts/latest", GetThreadsPostsLatestAsync)
                .WithSummary("Get the latest posts for threads");
            api.MapPost("{threadId}/request-approval", RequestThreadApprovalAsync)
                .WithSummary("Request thread approval");
            api.MapPost("{threadId}/approve", ApproveThreadAsync).WithSummary("Approve a thread");
            api.MapPost("{threadId}/reject", RejectThreadAsync).WithSummary("Reject a thread");
            api.MapPost(string.Empty, CreateThreadAsync).WithSummary("Create a thread");
            api.MapPost("{threadId}/posts", CreatePostAsync).WithSummary("Create a post");

            return app;
        }

        private IEndpointRouteBuilder PostApi()
        {
            var api = app
                .MapGroup("api/posts")
                .WithTags(nameof(PostApi))
                .WithAutoNames();

            api.MapGet("{postId}", GetPostAsync).WithSummary("Get a post");
            api.MapGet("{postId}/order", GetPostIndexAsync).WithSummary("Get a post's position in its thread");
            api.MapPatch("{postId}", UpdatePostAsync).WithSummary("Update a post");
            api.MapDelete("{postId}", DeletePostAsync).WithSummary("Delete a post");
            return app;
        }

        private IEndpointRouteBuilder PostBookmarkApi()
        {
            var api = app
                .MapGroup("api/posts/bookmarks")
                .WithTags(nameof(PostBookmarkApi))
                .WithAutoNames();

            api.MapGet("/bulk/{postIds}", GetBookmarkedPostIdsAsync)
                .WithSummary("Get bookmarked post IDs");
            api.MapPost("/{postId}", CreatePostBookmarkAsync).WithSummary("Bookmark a post");
            api.MapDelete("/{postId}", DeletePostBookmarkAsync).WithSummary("Remove a post bookmark");

            return app;
        }

        private IEndpointRouteBuilder UserBookmarkApi()
        {
            var api = app
                .MapGroup("api/users/{userId}/bookmarks")
                .WithTags(nameof(UserBookmarkApi))
                .WithAutoNames();

            api.MapGet("/count", GetBookmarkedPostsCountAsync).WithSummary("Get a user's bookmark count");
            api.MapGet(string.Empty, GetBookmarkedPostsPagedAsync).WithSummary("Get a user's bookmarked posts");

            return app;
        }

        private IEndpointRouteBuilder SearchApi()
        {
            var api = app
                .MapGroup("api/search")
                .WithTags(nameof(SearchApi))
                .WithAutoNames();

            api.MapGet(string.Empty, SearchAsync).WithSummary("Search forum content");

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
