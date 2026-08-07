using System.Security.Cryptography;
using System.Text.Json;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Shared.Application.Enums;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class SearchReadRepository : ISearchReadRepository
{
    private const string SearchCursorProtectorPurpose = "CoreService.SearchCursor.v1";

    private readonly ReadApplicationDbContext _dbContext;
    private readonly IDataProtector _cursorProtector;

    public SearchReadRepository(ReadApplicationDbContext dbContext, IDataProtectionProvider dataProtectionProvider)
    {
        _dbContext = dbContext;
        _cursorProtector = dataProtectionProvider.CreateProtector(SearchCursorProtectorPurpose);
    }

    public async Task<Result<SearchResultsDto, InvalidSearchCursorError, InvalidSearchPaginationError>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken)
    {
        var cursor = SearchCursorPayload.Decode(query.Cursor, query, _cursorProtector);
        if (query.Cursor is not null && cursor is null) return new InvalidSearchCursorError();

        await using var dataContext = _dbContext.CreateLinqToDBContext();
        var term = query.Term.Value;
        var searchQuery = dataContext
            .SelectQuery(() => new SearchQueryRow
            {
                TsQuery = IsPrefixSearch(query.Term)
                    ? PostgreSqlFullTextSearch.PrefixToTsQuery(term)
                    : PostgreSqlFullTextSearch.WebSearchToTsQuery(term)
            })
            .AsCte("search_query");

        var forumResults =
            from forum in _dbContext.Forums
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(forum, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            select new
            {
                Result = new SearchResultDto
                {
                    Type = SearchResultType.Forum,
                    ForumId = forum.ForumId,
                    CategoryId = null,
                    ThreadId = null,
                    PostId = null,
                    ForumTitle = forum.Title,
                    CategoryTitle = null,
                    ThreadTitle = null,
                    CreatedBy = forum.CreatedBy,
                    CreatedAt = forum.CreatedAt,
                    Snippet = null
                },
                Rank = vector.Rank(search.TsQuery),
                SortType = (byte)SearchResultType.Forum,
                SortKey = (Guid)forum.ForumId
            };

        var categoryResults =
            from category in _dbContext.Categories
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(category, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            select new
            {
                Result = new SearchResultDto
                {
                    Type = SearchResultType.Category,
                    ForumId = forum.ForumId,
                    CategoryId = category.CategoryId,
                    ThreadId = null,
                    PostId = null,
                    ForumTitle = forum.Title,
                    CategoryTitle = category.Title,
                    ThreadTitle = null,
                    CreatedBy = category.CreatedBy,
                    CreatedAt = category.CreatedAt,
                    Snippet = null
                },
                Rank = vector.Rank(search.TsQuery),
                SortType = (byte)SearchResultType.Category,
                SortKey = (Guid)category.CategoryId
            };

        var threadResults =
            from thread in _dbContext.Threads
            join category in _dbContext.Categories on thread.CategoryId equals category.CategoryId
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(thread, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            where thread.CanReadThread(query.QueriedBy)
            select new
            {
                Result = new SearchResultDto
                {
                    Type = SearchResultType.Thread,
                    ForumId = forum.ForumId,
                    CategoryId = category.CategoryId,
                    ThreadId = thread.ThreadId,
                    PostId = null,
                    ForumTitle = forum.Title,
                    CategoryTitle = category.Title,
                    ThreadTitle = thread.Title,
                    CreatedBy = thread.CreatedBy,
                    CreatedAt = thread.CreatedAt,
                    Snippet = null
                },
                Rank = vector.Rank(search.TsQuery),
                SortType = (byte)SearchResultType.Thread,
                SortKey = (Guid)thread.ThreadId
            };

        var postResults =
            from post in _dbContext.Posts
            join thread in _dbContext.Threads on post.ThreadId equals thread.ThreadId
            join category in _dbContext.Categories on thread.CategoryId equals category.CategoryId
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(post, Constants.SearchVectorColumnName)
            let searchText = Sql.Property<string>(post, Constants.SearchTextColumnName)
            where vector.Matches(search.TsQuery)
            where thread.CanReadThread(query.QueriedBy)
            select new
            {
                Result = new SearchResultDto
                {
                    Type = SearchResultType.Post,
                    ForumId = forum.ForumId,
                    CategoryId = category.CategoryId,
                    ThreadId = thread.ThreadId,
                    PostId = post.PostId,
                    ForumTitle = forum.Title,
                    CategoryTitle = category.Title,
                    ThreadTitle = thread.Title,
                    CreatedBy = post.CreatedBy,
                    CreatedAt = post.CreatedAt,
                    Snippet = PostgreSqlFullTextSearch.Headline(searchText, search.TsQuery)
                },
                Rank = vector.Rank(search.TsQuery),
                SortType = (byte)SearchResultType.Post,
                SortKey = (Guid)post.PostId
            };

        var results = query.Type switch
        {
            null => forumResults
                .Concat(categoryResults)
                .Concat(threadResults)
                .Concat(postResults),
            SearchResultType.Forum => forumResults,
            SearchResultType.Category => categoryResults,
            SearchResultType.Thread => threadResults,
            SearchResultType.Post => postResults,
            _ => throw new ArgumentOutOfRangeException(nameof(query.Type), query.Type, null)
        };

        if (cursor is not null)
        {
            var cursorSortKey = GetCursorSortKey(cursor);
            var isRelevanceSort = query.Sort.Field == SearchQuerySortType.Relevance;
            var cursorResultType = (byte)cursor.ResultType;
            var cursorQuery = dataContext
                .SelectQuery(() => new SearchCursorQueryRow
                {
                    SortType = cursorResultType,
                    SortKey = cursorSortKey
                })
                .AsCte("search_cursor");

            results = query.Sort.Order switch
            {
                SortOrderType.Descending =>
                    from result in results
                    from cursorValue in cursorQuery
                    where (isRelevanceSort && result.Rank < cursor.Rank) ||
                          ((!isRelevanceSort || result.Rank == cursor.Rank) &&
                           (result.Result.CreatedAt < cursor.CreatedAt ||
                            (result.Result.CreatedAt == cursor.CreatedAt &&
                             Sql.Row(result.SortType, result.SortKey) <
                             Sql.Row(cursorValue.SortType, cursorValue.SortKey))))
                    select result,
                SortOrderType.Ascending =>
                    from result in results
                    from cursorValue in cursorQuery
                    where (isRelevanceSort && result.Rank > cursor.Rank) ||
                          ((!isRelevanceSort || result.Rank == cursor.Rank) &&
                           (result.Result.CreatedAt > cursor.CreatedAt ||
                            (result.Result.CreatedAt == cursor.CreatedAt &&
                             Sql.Row(result.SortType, result.SortKey) >
                             Sql.Row(cursorValue.SortType, cursorValue.SortKey))))
                    select result,
                _ => throw new ArgumentOutOfRangeException(nameof(query.Sort.Order), query.Sort.Order, null)
            };
        }

        var orderedResults = (query.Sort.Field, query.Sort.Order) switch
        {
            (SearchQuerySortType.Relevance, SortOrderType.Descending) => results.OrderByDescending(result => new
            {
                result.Rank,
                result.Result.CreatedAt,
                result.SortType,
                result.SortKey
            }),
            (SearchQuerySortType.Newest, SortOrderType.Descending) => results.OrderByDescending(result => new
            {
                result.Result.CreatedAt,
                result.SortType,
                result.SortKey
            }),
            (SearchQuerySortType.Relevance, SortOrderType.Ascending) => results.OrderBy(result => new
            {
                result.Rank,
                result.Result.CreatedAt,
                result.SortType,
                result.SortKey
            }),
            (SearchQuerySortType.Newest, SortOrderType.Ascending) => results.OrderBy(result => new
            {
                result.Result.CreatedAt,
                result.SortType,
                result.SortKey
            }),
            _ => throw new ArgumentOutOfRangeException(nameof(query.Sort), query.Sort, null)
        };
        var pagedResults = cursor is null && query.Offset != 0
            ? orderedResults.Skip(query.Offset.Value)
            : orderedResults;

        var rows = await pagedResults
            .Take(query.Limit.Value + 1)
            .ToListAsyncLinqToDB(cancellationToken);

        var hasMore = rows.Count > query.Limit.Value;
        if (hasMore) rows.RemoveAt(query.Limit.Value);

        var last = rows.LastOrDefault();
        var items = rows.ConvertAll(static result => result.Result);

        return new SearchResultsDto
        {
            Items = items,
            NextCursor = hasMore && last != null
                ? CreateCursor(last.Result, last.Rank, query)
                : null
        };
    }

    private static bool IsPrefixSearch(SearchTerm term) =>
        term.Value.Length >= 4 && term.Value.All(char.IsLetter);

    private static Guid GetCursorSortKey(SearchCursorPayload cursor) => cursor.ResultType switch
    {
        SearchResultType.Forum => (Guid)cursor.ForumId,
        SearchResultType.Category => (Guid)cursor.CategoryId!.Value,
        SearchResultType.Thread => (Guid)cursor.ThreadId!.Value,
        SearchResultType.Post => (Guid)cursor.PostId!.Value,
        _ => throw new InvalidOperationException($"Unsupported search result type: {cursor.ResultType}")
    };

    private SearchCursor CreateCursor(SearchResultDto row, float rank, SearchQuery query) =>
        SearchCursor.From(SearchCursorPayload.Encode(row, rank, query, _cursorProtector));

    private sealed class SearchQueryRow
    {
        public NpgsqlTsQuery TsQuery { get; init; } = null!;
    }

    private sealed class SearchCursorQueryRow
    {
        public byte SortType { get; init; }
        public Guid SortKey { get; init; }
    }

    private sealed record SearchCursorPayload(
        byte Version,
        SearchTerm Term,
        SearchResultType? Type,
        SearchQuerySortType SortField,
        SortOrderType SortOrder,
        UserIdRole? QueriedBy,
        float Rank,
        DateTime CreatedAt,
        SearchResultType ResultType,
        ForumId ForumId,
        CategoryId? CategoryId,
        ThreadId? ThreadId,
        PostId? PostId)
    {
        private const byte CurrentVersion = 1;

        public static string Encode(SearchResultDto row, float rank, SearchQuery query, IDataProtector cursorProtector)
        {
            var hasCategory = row.Type is not SearchResultType.Forum;
            var hasThread = row.Type is SearchResultType.Thread or SearchResultType.Post;
            var hasPost = row.Type is SearchResultType.Post;
            var payload = JsonSerializer.SerializeToUtf8Bytes(new SearchCursorPayload(
                CurrentVersion,
                query.Term,
                query.Type,
                query.Sort.Field,
                query.Sort.Order,
                query.QueriedBy,
                rank,
                row.CreatedAt,
                row.Type,
                row.ForumId,
                hasCategory ? row.CategoryId : null,
                hasThread ? row.ThreadId : null,
                hasPost ? row.PostId : null));

            return WebEncoders.Base64UrlEncode(cursorProtector.Protect(payload));
        }

        public static SearchCursorPayload? Decode(
            SearchCursor? cursor,
            SearchQuery query,
            IDataProtector cursorProtector)
        {
            if (cursor is null) return null;

            try
            {
                var bytes = cursorProtector.Unprotect(WebEncoders.Base64UrlDecode(cursor.Value.Value));
                var payload = JsonSerializer.Deserialize<SearchCursorPayload>(bytes);
                return payload is { IsValid: true } && payload.IsFor(query) ? payload : null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (CryptographicException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private bool IsFor(SearchQuery query) =>
            Term == query.Term &&
            Type == query.Type &&
            SortField == query.Sort.Field &&
            SortOrder == query.Sort.Order &&
            QueriedBy == query.QueriedBy;

        private bool IsValid =>
            Version == CurrentVersion &&
            !string.IsNullOrWhiteSpace(Term.Value) &&
            SortField is SearchQuerySortType.Relevance or SearchQuerySortType.Newest &&
            SortOrder is SortOrderType.Ascending or SortOrderType.Descending &&
            ForumId.Value != Guid.Empty &&
            ResultType switch
            {
                SearchResultType.Forum => true,
                SearchResultType.Category =>
                    CategoryId is { } categoryId && categoryId.Value != Guid.Empty,
                SearchResultType.Thread =>
                    CategoryId is { } threadCategoryId && threadCategoryId.Value != Guid.Empty &&
                    ThreadId is { } threadId && threadId.Value != Guid.Empty,
                SearchResultType.Post =>
                    CategoryId is { } postCategoryId && postCategoryId.Value != Guid.Empty &&
                    ThreadId is { } postThreadId && postThreadId.Value != Guid.Empty &&
                    PostId is { } postId && postId.Value != Guid.Empty,
                _ => false
            };
    }
}
