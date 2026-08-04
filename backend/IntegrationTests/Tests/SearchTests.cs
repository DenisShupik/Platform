using System.Net;
using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Application.ValueObjects;

namespace IntegrationTests.Tests;

public sealed class SearchTests
{
    [ClassDataSource<CoreServiceTestsFixture<SearchTests>>(Shared = SharedType.PerClass)]
    public required CoreServiceTestsFixture<SearchTests> Fixture { get; init; }

    [Test]
    public async Task Search_FindsWordsByPrefix(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();
        var forumId = await moderatorClient.CreateForumAsync(new CreateForumRequestBody
        {
            Title = ForumTitle.From("Обсуждение производительности")
        }, cancellationToken);

        var results = await anonymousClient.SearchAsync(
            SearchTerm.From("обсужден"),
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            cancellationToken);

        await Assert.That(results.Items.Any(item => item.ForumId == forumId)).IsTrue();
    }

    [Test]
    public async Task Search_ReturnsMixedResultTypes(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var userClient = Fixture.GetCoreServiceClient(Fixture.TestUsername);
        const string termValue = "смешанныйпоиск";
        var term = SearchTerm.From(termValue);
        var forumId = await moderatorClient.CreateForumAsync(new CreateForumRequestBody
        {
            Title = ForumTitle.From($"{termValue} форума")
        }, cancellationToken);
        var categoryId = await moderatorClient.CreateCategoryAsync(new CreateCategoryRequestBody
        {
            ForumId = forumId,
            Title = CategoryTitle.From($"{termValue} раздела")
        }, cancellationToken);
        var threadId = await userClient.CreateThreadAsync(new CreateThreadRequestBody
        {
            CategoryId = categoryId,
            Title = ThreadTitle.From($"{termValue} темы")
        }, cancellationToken);
        var postId = await userClient.CreatePostAsync(threadId, new CreatePostRequestBody
        {
            Content = PostContent.From($"{termValue} сообщения")
        }, cancellationToken);

        var results = await userClient.SearchAsync(term, null, SearchSortDefaults.Relevance, cancellationToken);

        await Assert.That(results.Items.Select(item => item.Type)).Contains(SearchResultType.Forum);
        await Assert.That(results.Items.Select(item => item.Type)).Contains(SearchResultType.Category);
        await Assert.That(results.Items.Select(item => item.Type)).Contains(SearchResultType.Thread);
        await Assert.That(results.Items.Select(item => item.Type)).Contains(SearchResultType.Post);

        var forumResult = results.Items.Single(item => item.Type == SearchResultType.Forum && item.ForumId == forumId);
        var categoryResult = results.Items.Single(item => item.Type == SearchResultType.Category && item.CategoryId == categoryId);
        var threadResult = results.Items.Single(item => item.Type == SearchResultType.Thread && item.ThreadId == threadId);
        var postResult = results.Items.Single(item => item.Type == SearchResultType.Post && item.PostId == postId);

        await Assert.That(forumResult.CategoryId).IsNull();
        await Assert.That(forumResult.ThreadId).IsNull();
        await Assert.That(forumResult.PostId).IsNull();
        await Assert.That(categoryResult.ForumId).IsEqualTo(forumId);
        await Assert.That(categoryResult.ThreadId).IsNull();
        await Assert.That(categoryResult.PostId).IsNull();
        await Assert.That(threadResult.ForumId).IsEqualTo(forumId);
        await Assert.That(threadResult.CategoryId).IsEqualTo(categoryId);
        await Assert.That(threadResult.PostId).IsNull();
        await Assert.That(postResult.ForumId).IsEqualTo(forumId);
        await Assert.That(postResult.CategoryId).IsEqualTo(categoryId);
        await Assert.That(postResult.ThreadId).IsEqualTo(threadId);
    }

    [Test]
    public async Task Search_CursorUsesValueObjectsAndDoesNotRepeatResults(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();
        const string termValue = "страницыпоиска";
        var term = SearchTerm.From(termValue);

        for (var index = 0; index < 21; index++)
        {
            await moderatorClient.CreateForumAsync(new CreateForumRequestBody
            {
                Title = ForumTitle.From($"{termValue} {index}")
            }, cancellationToken);
        }

        var firstPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            cancellationToken);

        await Assert.That(firstPage.Items.Count).IsEqualTo(20);
        await Assert.That(firstPage.NextCursor).IsNotNull();

        var secondPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            firstPage.NextCursor,
            cancellationToken);

        var ids = firstPage.Items.Concat(secondPage.Items).Select(item => item.ForumId).ToList();
        await Assert.That(secondPage.Items).IsNotEmpty();
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);

        var firstAscendingPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.RelevanceAscending,
            cancellationToken);

        var secondAscendingPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.RelevanceAscending,
            firstAscendingPage.NextCursor,
            cancellationToken);

        var ascendingIds = firstAscendingPage.Items
            .Concat(secondAscendingPage.Items)
            .Select(item => item.ForumId)
            .ToList();
        await Assert.That(firstAscendingPage.NextCursor).IsNotNull();
        await Assert.That(secondAscendingPage.Items).IsNotEmpty();
        await Assert.That(ascendingIds.Distinct().Count()).IsEqualTo(ascendingIds.Count);
    }

    [Test]
    public async Task Search_RejectsCursorOutsideItsOriginalScope(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();
        const string termValue = "областьпоиска";
        var term = SearchTerm.From(termValue);

        for (var index = 0; index < 21; index++)
        {
            await moderatorClient.CreateForumAsync(new CreateForumRequestBody
            {
                Title = ForumTitle.From($"{termValue} {index}")
            }, cancellationToken);
        }

        var anonymousFirstPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            cancellationToken);
        var moderatorFirstPage = await moderatorClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            cancellationToken);

        await Assert.That(anonymousFirstPage.NextCursor).IsNotNull();
        await Assert.That(moderatorFirstPage.NextCursor).IsNotNull();

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            SearchTerm.From("другойпоиск"),
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            anonymousFirstPage.NextCursor,
            cancellationToken));

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.RelevanceAscending,
            anonymousFirstPage.NextCursor,
            cancellationToken));

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            term,
            null,
            SearchSortDefaults.Relevance,
            anonymousFirstPage.NextCursor,
            cancellationToken));

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            moderatorFirstPage.NextCursor,
            cancellationToken));
    }

    [Test]
    public async Task Search_RejectsTamperedCursorAndCursorWithOffset(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();
        const string termValue = "защитакурсора";
        var term = SearchTerm.From(termValue);

        for (var index = 0; index < 21; index++)
        {
            await moderatorClient.CreateForumAsync(new CreateForumRequestBody
            {
                Title = ForumTitle.From($"{termValue} {index}")
            }, cancellationToken);
        }

        var firstPage = await anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            cancellationToken);
        await Assert.That(firstPage.NextCursor).IsNotNull();

        var cursorValue = firstPage.NextCursor!.Value.Value;
        var tamperedCursor = SearchCursor.From(
            (cursorValue[0] == 'A' ? "B" : "A") + cursorValue[1..]);

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            tamperedCursor,
            cancellationToken));

        await AssertBadRequestAsync(() => anonymousClient.SearchAsync(
            term,
            SearchResultType.Forum,
            SearchSortDefaults.Relevance,
            firstPage.NextCursor,
            cancellationToken,
            PaginationOffset.From(1)));
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

        throw new InvalidOperationException("Search request was expected to return BadRequest.");
    }
}

file static class SearchSortDefaults
{
    public static readonly SortCriteria<SearchQuerySortType> Relevance = new()
    {
        Field = SearchQuerySortType.Relevance,
        Order = SortOrderType.Descending
    };

    public static readonly SortCriteria<SearchQuerySortType> RelevanceAscending = new()
    {
        Field = SearchQuerySortType.Relevance,
        Order = SortOrderType.Ascending
    };
}
