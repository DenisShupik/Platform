using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.Abstractions;
using Shared.Domain.Enums;
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

        using var scope = Fixture.Services.CreateScope();
        var queriedBy = new UserIdRole(Fixture.TestUserId, Role.User);
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
                ThreadIds = new IdSet<ThreadId, Guid>([threadId, emptyThreadId, missingThreadId]),
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
    }
}
