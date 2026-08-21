using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Presentation.Rest.Dtos;
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
                .WithTags(nameof(ForumApi));

            api.MapGet<GetForumsPagedRequest, GetForumsPagedQueryHandler<ForumDto>>(string.Empty);
            api.MapGet<GetForumsCountRequest, GetForumsCountQueryHandler>("count");
            api.MapGet<GetForumRequest, GetForumQueryHandler<ForumDto>>("{forumId}");
            api.MapGet<GetForumAllowedActionsRequest, GetForumAllowedActionsQueryHandler>(
                "{forumId}/allowed-actions");
            api.MapGet<GetForumModeratorsRequest, GetForumModeratorsQueryHandler>("{forumId}/moderators");
            api.MapGet<GetForumsBulkRequest, GetForumsBulkQueryHandler<ForumDto>>("bulk/{forumIds}");
            api.MapGet<GetForumsCategoriesCountRequest, GetForumsCategoriesCountQueryHandler>(
                "{forumIds}/categories/count");
            api.MapPost<CreateForumRequest, CreateForumCommandHandler>(string.Empty);
            api.MapPost<AppointForumModeratorRequest, AppointForumModeratorCommandHandler>(
                "{forumId}/moderators/{userId}");
            api.MapDelete<RevokeForumModeratorRequest, RevokeForumModeratorCommandHandler>(
                "{forumId}/moderators/{userId}");

            return app;
        }

        private IEndpointRouteBuilder AuthorizationApi()
        {
            var api = app
                .MapGroup("api/authorization")
                .WithTags(nameof(AuthorizationApi));

            api.MapGet<GetPlatformAllowedActionsRequest, GetPlatformAllowedActionsQueryHandler>(
                "platform/allowed-actions");
            api.MapGet<GetAdministrationAllowedActionsRequest, GetAdministrationAllowedActionsQueryHandler>(
                "administration/allowed-actions");
            api.MapGet<GetPlatformAdministratorsRequest, GetPlatformAdministratorsQueryHandler>(
                "platform/administrators");
            api.MapPost<AppointPlatformAdministratorRequest, AppointPlatformAdministratorCommandHandler>(
                "platform/administrators/{userId}");
            api.MapDelete<RevokePlatformAdministratorRequest, RevokePlatformAdministratorCommandHandler>(
                "platform/administrators/{userId}");
            api.MapGet<GetCapabilityGrantsRequest, GetCapabilityGrantsQueryHandler>("grants");
            api.MapGet<GetCapabilityCatalogRequest, GetCapabilityCatalogQueryHandler>("capabilities");
            api.MapGet<GetEffectiveCapabilityGrantsRequest, GetEffectiveCapabilityGrantsQueryHandler>(
                "users/{userId}/effective-grants");
            api.MapPost<GrantCapabilityRequest, GrantCapabilityCommandHandler>("grants");
            api.MapDelete<RevokeCapabilityRequest, RevokeCapabilityCommandHandler>(
                "grants/{capabilityGrantId}");
            api.MapGet<GetForumSanctionsRequest, GetForumSanctionsQueryHandler>("sanctions");
            api.MapPost<IssueForumSanctionRequest, IssueForumSanctionCommandHandler>("sanctions");
            api.MapDelete<RevokeForumSanctionRequest, RevokeForumSanctionCommandHandler>(
                "sanctions/{forumSanctionId}");

            return app;
        }

        private IEndpointRouteBuilder CategoryApi()
        {
            var api = app
                .MapGroup("api/categories")
                .WithTags(nameof(CategoryApi));

            api.MapGet<GetCategoriesPagedRequest, GetCategoriesPagedQueryHandler<CategoryDto>>(string.Empty);
            api.MapGet<GetCategoryRequest, GetCategoryQueryHandler<CategoryDto>>("{categoryId}");
            api.MapGet<GetCategoriesBulkRequest, GetCategoriesBulkQueryHandler<CategoryDto>>("bulk/{categoryIds}");
            api.MapGet<GetCategoriesPostsCountRequest, GetCategoriesPostsCountQueryHandler>(
                "{categoryIds}/posts/count");
            api.MapGet<GetCategoriesPostsLatestRequest, GetCategoriesPostsLatestQueryHandler<PostDto>>(
                "{categoryIds}/posts/latest");
            api.MapGet<GetCategoriesThreadsCountRequest, GetCategoriesThreadsCountQueryHandler>(
                "{categoryIds}/threads/count");
            api.MapGet<GetCategoryThreadsPagedRequest, GetCategoryThreadsPagedQueryHandler<ThreadDto>>(
                "{categoryId}/threads");
            api.MapGet<GetCategoryAllowedActionsRequest, GetCategoryAllowedActionsQueryHandler>(
                "{categoryId}/allowed-actions");
            api.MapGet<GetCategoryModeratorsRequest, GetCategoryModeratorsQueryHandler>(
                "{categoryId}/moderators");
            api.MapPost<CreateCategoryRequest, CreateCategoryCommandHandler>(string.Empty);
            api.MapPost<AppointCategoryModeratorRequest, AppointCategoryModeratorCommandHandler>(
                "{categoryId}/moderators/{userId}");
            api.MapDelete<RevokeCategoryModeratorRequest, RevokeCategoryModeratorCommandHandler>(
                "{categoryId}/moderators/{userId}");

            return app;
        }

        private IEndpointRouteBuilder ThreadApi()
        {
            var api = app
                .MapGroup("api/threads")
                .WithTags(nameof(ThreadApi));

            api.MapGet<GetThreadsPagedRequest, GetThreadsPagedQueryHandler<ThreadDto>>(string.Empty);
            api.MapGet<GetThreadsCountRequest, GetThreadsCountQueryHandler>("count");
            api.MapGet<GetThreadRequest, GetThreadQueryHandler<ThreadDto>>("{threadId}");
            api.MapGet<GetThreadAllowedActionsRequest, GetThreadAllowedActionsQueryHandler>(
                "{threadId}/allowed-actions");
            api.MapGet<GetThreadsBulkRequest, GetThreadsBulkQueryHandler<ThreadDto>>("bulk/{threadIds}");
            api.MapGet<GetThreadPostsPagedRequest, GetThreadPostsPagedQueryHandler<PostDto>>("{threadId}/posts");
            api.MapGet<GetThreadsPostsCountRequest, GetThreadsPostsCountQueryHandler>("{threadIds}/posts/count");
            api.MapGet<GetThreadsPostsLatestRequest, GetThreadsPostsLatestQueryHandler<PostDto>>(
                "{threadIds}/posts/latest");
            api.MapPost<RequestThreadApprovalRequest, RequestThreadApprovalCommandHandler>(
                "{threadId}/request-approval");
            api.MapPost<ApproveThreadRequest, ApproveThreadCommandHandler>("{threadId}/approve");
            api.MapPost<RejectThreadRequest, RejectThreadCommandHandler>("{threadId}/reject");
            api.MapPost<CreateThreadRequest, CreateThreadCommandHandler>(string.Empty);
            api.MapPost<CreatePostRequest, CreatePostCommandHandler>("{threadId}/posts");

            return app;
        }

        private IEndpointRouteBuilder PostApi()
        {
            var api = app
                .MapGroup("api/posts")
                .WithTags(nameof(PostApi));

            api.MapGet<GetPostRequest, GetPostQueryHandler<PostDto>>("{postId}");
            api.MapGet<GetPostIndexRequest, GetPostIndexQueryHandler>("{postId}/order");
            api.MapPatch<UpdatePostRequest, UpdatePostCommandHandler>("{postId}");
            api.MapDelete<DeletePostRequest, DeletePostCommandHandler>("{postId}");
            return app;
        }

        private IEndpointRouteBuilder PostBookmarkApi()
        {
            var api = app
                .MapGroup("api/posts/bookmarks")
                .WithTags(nameof(PostBookmarkApi));

            api.MapGet<GetBookmarkedPostIdsBulkRequest, GetBookmarkedPostIdsBulkQueryHandler>("bulk/{postIds}");
            api.MapPost<CreatePostBookmarkRequest, CreatePostBookmarkCommandHandler>("{postId}");
            api.MapDelete<DeletePostBookmarkRequest, DeletePostBookmarkCommandHandler>("{postId}");

            return app;
        }

        private IEndpointRouteBuilder UserBookmarkApi()
        {
            var api = app
                .MapGroup("api/users/{userId}/bookmarks")
                .WithTags(nameof(UserBookmarkApi));

            api.MapGet<GetBookmarkedPostsCountRequest, GetBookmarkedPostsCountQueryHandler>("count");
            api.MapGet<GetBookmarkedPostsPagedRequest, GetBookmarkedPostsPagedQueryHandler<PostDto>>(string.Empty);

            return app;
        }

        private IEndpointRouteBuilder SearchApi()
        {
            var api = app
                .MapGroup("api/search")
                .WithTags(nameof(SearchApi));

            api.MapGet<SearchRequest, SearchQueryHandler>(string.Empty);

            return app;
        }

        public IEndpointRouteBuilder MapApi()
        {
            app
                .ForumApi()
                .AuthorizationApi()
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
