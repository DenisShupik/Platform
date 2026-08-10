using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using CoreService.Application.Diagnostics;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence;
using CoreService.Presentation.Rest.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class CreatePostTests
{
    [ClassDataSource<CoreServiceTestsFixture<CreatePostTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<CreatePostTests> Fixture { get; init; }

    [Test]
    public async Task ParallelCreatePosts_Success(CancellationToken cancellationToken)
    {
        const int parallelPostCount = 10;
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);

        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);

        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await userClient.CreatePostAsync(threadId, TestRequests.CreateHeaderPost, cancellationToken);

        await userClient.RequestThreadApprovalAsync(threadId, cancellationToken);

        await moderatorClient.ApproveThreadAsync(threadId, cancellationToken);

        var completedActivities = new ConcurrentBag<string>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == CoreServiceActivitySource.SourceName,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            SampleUsingParentId = (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity =>
            {
                if (Equals(
                        activity.GetTagItem(CoreServiceActivitySource.ThreadIdTagName),
                        threadId.ToString()))
                {
                    completedActivities.Add(activity.OperationName);
                }
            }
        };
        ActivitySource.AddActivityListener(activityListener);

        var tasks = Enumerable.Range(0, parallelPostCount)
            .Select(_ => userClient.CreatePostAsync(
                threadId,
                TestRequests.CreatePost,
                cancellationToken));
        var createdPostIds = await Task.WhenAll(tasks);

        await using var scope = Fixture.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ReadApplicationDbContext>();
        var thread = await dbContext.Threads
            .AsNoTracking()
            .SingleAsync(candidate => candidate.ThreadId == threadId, cancellationToken);
        var persistedPostCount = await dbContext.Posts
            .CountAsync(post => post.ThreadId == threadId, cancellationToken);
        var persistedCreatedPostCount = await dbContext.Posts
            .CountAsync(post => createdPostIds.Contains(post.PostId), cancellationToken);

        await Assert.That(thread.PostCount).IsEqualTo(Count.From(parallelPostCount + 1));
        await Assert.That(persistedPostCount).IsEqualTo(parallelPostCount + 1);
        await Assert.That(createdPostIds.Distinct().Count()).IsEqualTo(parallelPostCount);
        await Assert.That(persistedCreatedPostCount).IsEqualTo(parallelPostCount);

        foreach (var activityName in CreatePostActivityNames)
        {
            await Assert.That(completedActivities.Count(name => name == activityName))
                .IsEqualTo(parallelPostCount);
        }
    }

    [Test]
    public async Task CreatePost_RejectsRawHtmlAndUnsupportedLinks(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        var forumId = await moderatorClient.CreateForumAsync(TestRequests.CreateForum, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(TestRequests.CreateCategory(forumId), cancellationToken);
        var threadId = await userClient.CreateThreadAsync(TestRequests.CreateThread(categoryId), cancellationToken);

        await AssertBadRequestAsync(
            () => userClient.CreatePostAsync(threadId,
                new CreatePostRequestBody { Content = PostContent.From("<script>alert(1)</script>") },
                cancellationToken));
        await AssertBadRequestAsync(
            () => userClient.CreatePostAsync(threadId,
                new CreatePostRequestBody { Content = PostContent.From("[external](ftp://example.org)") },
                cancellationToken));
    }

    private static async Task AssertBadRequestAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.BadRequest)
        {
            return;
        }

        throw new InvalidOperationException("Post creation was expected to return BadRequest.");
    }

    private static readonly string[] CreatePostActivityNames =
    [
        CoreServiceActivitySource.PreparePostContent,
        CoreServiceActivitySource.BeginPostTransaction,
        CoreServiceActivitySource.LoadThreadForPost,
        CoreServiceActivitySource.HoldThreadLockForPost,
        CoreServiceActivitySource.AddPostToThread,
        CoreServiceActivitySource.PublishPostAdded,
        CoreServiceActivitySource.CommitPost
    ];
}
