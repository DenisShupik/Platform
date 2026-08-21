using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class LatestPostProjectionTests
{
    [ClassDataSource<CoreServiceTestsFixture<LatestPostProjectionTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<LatestPostProjectionTests> Fixture { get; init; }

    [Test]
    public async Task LatestPostQueries_ProjectTheLatestPostDirectly(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId),
            cancellationToken);
        var threadId = await userClient.CreateThreadAsync(
            TestRequests.CreateThread(categoryId),
            cancellationToken);
        var emptyThreadId = await userClient.CreateThreadAsync(
            TestRequests.CreateThread(categoryId, "Empty thread"),
            cancellationToken);
        await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var latestPostId = await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var inaccessibleThreadId = await moderatorClient.CreateThreadAsync(
            TestRequests.CreateThread(categoryId, "Another user's draft"),
            cancellationToken);
        await moderatorClient.CreatePostAsync(inaccessibleThreadId, TestRequests.CreatePost, cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var queriedBy = new ActorContext(Fixture.TestUserId);
        var categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryReadRepository>();
        var categoryPosts = await categoryRepository.GetCategoriesPostsLatestAsync<PostDto>(
            new GetCategoriesPostsLatestQuery<PostDto>
            {
                CategoryIds = new IdSet<CategoryId, Guid>([categoryId]),
                QueriedBy = queriedBy
            },
            cancellationToken);

        var threadRepository = scope.ServiceProvider.GetRequiredService<IThreadReadRepository>();
        var missingThreadId = ThreadId.From(Guid.NewGuid());
        var threadPosts = await threadRepository.GetThreadsPostsLatestAsync<PostDto>(
            new GetThreadsPostsLatestQuery<PostDto>
            {
                ThreadIds = new IdSet<ThreadId, Guid>(
                    [threadId, emptyThreadId, inaccessibleThreadId, missingThreadId]),
                QueriedBy = queriedBy
            },
            cancellationToken);

        await Assert.That(categoryPosts[categoryId].PostId).IsEqualTo(latestPostId);

        var latestThreadPost = threadPosts[threadId].Match(
            value => value,
            _ => throw new InvalidOperationException("Expected the latest post."),
            _ => throw new InvalidOperationException("Expected the latest post."),
            _ => throw new InvalidOperationException("Expected the latest post."));
        await Assert.That(latestThreadPost.PostId).IsEqualTo(latestPostId);

        var emptyThreadError = threadPosts[emptyThreadId].Match<object>(
            _ => throw new InvalidOperationException("Expected PostNotFoundError."),
            error => error,
            error => error,
            error => error);
        await Assert.That(emptyThreadError).IsTypeOf<PostNotFoundError>();

        var missingThreadError = threadPosts[missingThreadId].Match<object>(
            _ => throw new InvalidOperationException("Expected ThreadNotFoundError."),
            error => error,
            error => error,
            error => error);
        await Assert.That(missingThreadError).IsTypeOf<ThreadNotFoundError>();

        var inaccessibleThreadError = threadPosts[inaccessibleThreadId].Match<object>(
            _ => throw new InvalidOperationException("Expected PermissionDeniedError."),
            error => error,
            error => error,
            error => error);
        await Assert.That(inaccessibleThreadError).IsTypeOf<PermissionDeniedError>();
    }

    [Test]
    public async Task PagedQueries_CombineExistenceAndAccessWithDataRead(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId),
            cancellationToken);
        var emptyCategoryId = await moderatorClient.CreateCategoryAsync(
            TestRequests.CreateCategory(forumId),
            cancellationToken);
        var threadId = await userClient.CreateThreadAsync(
            TestRequests.CreateThread(categoryId),
            cancellationToken);
        await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        await userClient.CreatePostAsync(threadId, TestRequests.CreatePost, cancellationToken);
        var inaccessibleThreadId = await moderatorClient.CreateThreadAsync(
            TestRequests.CreateThread(categoryId, "Another user's draft"),
            cancellationToken);
        await moderatorClient.CreatePostAsync(inaccessibleThreadId, TestRequests.CreatePost, cancellationToken);

        using var scope = Fixture.Services.CreateScope();
        var queriedBy = new ActorContext(Fixture.TestUserId);
        var postRepository = scope.ServiceProvider.GetRequiredService<IPostReadRepository>();

        var postsResult = await postRepository.GetThreadPostsAsync<PostDto>(
            CreatePostsQuery(threadId, queriedBy),
            cancellationToken);
        var posts = postsResult.Match(
            value => value,
            _ => throw new InvalidOperationException("Expected posts."),
            _ => throw new InvalidOperationException("Expected posts."));
        await Assert.That(posts).Count().IsEqualTo(2);

        var emptyPageResult = await postRepository.GetThreadPostsAsync<PostDto>(
            CreatePostsQuery(threadId, queriedBy, PaginationOffset.From(100)),
            cancellationToken);
        var emptyPage = emptyPageResult.Match(
            value => value,
            _ => throw new InvalidOperationException("Expected an empty page."),
            _ => throw new InvalidOperationException("Expected an empty page."));
        await Assert.That(emptyPage).IsEmpty();

        var missingThreadResult = await postRepository.GetThreadPostsAsync<PostDto>(
            CreatePostsQuery(ThreadId.From(Guid.NewGuid()), queriedBy),
            cancellationToken);
        var missingThreadError = missingThreadResult.Match<object>(
            _ => throw new InvalidOperationException("Expected ThreadNotFoundError."),
            error => error,
            error => error);
        await Assert.That(missingThreadError).IsTypeOf<ThreadNotFoundError>();

        var inaccessibleThreadResult = await postRepository.GetThreadPostsAsync<PostDto>(
            CreatePostsQuery(inaccessibleThreadId, queriedBy),
            cancellationToken);
        var inaccessibleThreadError = inaccessibleThreadResult.Match<object>(
            _ => throw new InvalidOperationException("Expected PermissionDeniedError."),
            error => error,
            error => error);
        await Assert.That(inaccessibleThreadError).IsTypeOf<PermissionDeniedError>();

        var categoryRepository = scope.ServiceProvider.GetRequiredService<ICategoryReadRepository>();
        var threadsResult = await categoryRepository.GetCategoryThreadsAsync<ThreadDto>(
            CreateThreadsQuery(categoryId, queriedBy),
            cancellationToken);
        var threads = threadsResult.Match(
            value => value,
            _ => throw new InvalidOperationException("Expected threads."));
        await Assert.That(threads.Select(e => e.ThreadId)).IsEquivalentTo([threadId]);

        var emptyCategoryResult = await categoryRepository.GetCategoryThreadsAsync<ThreadDto>(
            CreateThreadsQuery(emptyCategoryId, queriedBy),
            cancellationToken);
        var emptyCategory = emptyCategoryResult.Match(
            value => value,
            _ => throw new InvalidOperationException("Expected an empty category."));
        await Assert.That(emptyCategory).IsEmpty();

        var missingCategoryResult = await categoryRepository.GetCategoryThreadsAsync<ThreadDto>(
            CreateThreadsQuery(CategoryId.From(Guid.NewGuid()), queriedBy),
            cancellationToken);
        var missingCategoryError = missingCategoryResult.Match<object>(
            _ => throw new InvalidOperationException("Expected CategoryNotFoundError."),
            error => error);
        await Assert.That(missingCategoryError).IsTypeOf<CategoryNotFoundError>();
    }

    private static GetThreadPostsPagedQuery<PostDto> CreatePostsQuery(
        ThreadId threadId,
        ActorContext queriedBy,
        PaginationOffset? offset = null) =>
        new()
        {
            ThreadId = threadId,
            QueriedBy = queriedBy,
            Offset = offset ?? PaginationOffset.Default,
            Limit = PaginationLimit.From(100),
            Sort = new SortCriteria<GetThreadPostsPagedQuerySortType>
            {
                Field = GetThreadPostsPagedQuerySortType.Index,
                Order = SortOrderType.Ascending
            }
        };

    private static GetCategoryThreadsPagedQuery<ThreadDto> CreateThreadsQuery(
        CategoryId categoryId,
        ActorContext queriedBy) =>
        new()
        {
            CategoryId = categoryId,
            State = null,
            QueriedBy = queriedBy,
            Offset = PaginationOffset.Default,
            Limit = PaginationLimit.From(100),
            Sort = new SortCriteria<GetCategoryThreadsPagedQuerySortType>
            {
                Field = GetCategoryThreadsPagedQuerySortType.Activity,
                Order = SortOrderType.Descending
            }
        };
}
