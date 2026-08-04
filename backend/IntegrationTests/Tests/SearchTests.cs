using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Presentation.Rest.Dtos;
using Shared.Application.Abstractions;
using Shared.Application.Enums;

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
    public async Task Search_CursorUsesValueObjectsAndDoesNotRepeatResults(CancellationToken cancellationToken)
    {
        var moderatorClient = Fixture.GetCoreServiceClient(Fixture.TestModeratorUsername);
        var anonymousClient = Fixture.GetCoreServiceClient();
        var term = SearchTerm.From("обсужден");

        for (var index = 0; index < 21; index++)
        {
            await moderatorClient.CreateForumAsync(new CreateForumRequestBody
            {
                Title = ForumTitle.From($"Обсуждение курсора {index}")
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
