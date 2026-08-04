using System.Linq.Expressions;
using System.Text.Json;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using Shared.Application.Abstractions;
using Shared.Application.Enums;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(SearchQuery), typeof(SearchResultDto))]
internal static partial class SearchReadRepositoryExtensions
{
    [SortExpression<SearchQuerySortType>(SearchQuerySortType.Relevance)]
    private static readonly Expression<Func<SearchResultDto, object>> RelevanceExpression = result => new
    {
        result.Rank,
        result.CreatedAt,
        result.Type,
        result.ForumId,
        result.CategoryId,
        result.ThreadId,
        result.PostId
    };

    [SortExpression<SearchQuerySortType>(SearchQuerySortType.Newest)]
    private static readonly Expression<Func<SearchResultDto, object>> NewestExpression = result => new
    {
        result.CreatedAt,
        result.Type,
        result.ForumId,
        result.CategoryId,
        result.ThreadId,
        result.PostId
    };
}

public sealed class SearchReadRepository : ISearchReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public SearchReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<SearchResultsDto, InvalidSearchCursorError>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken)
    {
        var cursor = SearchCursorPayload.Decode(query.Cursor);
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
            select new SearchResultDto
            {
                Type = SearchResultType.Forum,
                ForumId = forum.ForumId,
                ForumTitle = forum.Title,
                CategoryId = null,
                CategoryTitle = null,
                ThreadId = null,
                ThreadTitle = null,
                PostId = null,
                CreatedBy = forum.CreatedBy,
                CreatedAt = forum.CreatedAt,
                Snippet = null,
                Rank = vector.Rank(search.TsQuery)
            };

        var categoryResults =
            from category in _dbContext.Categories
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(category, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            select new SearchResultDto
            {
                Type = SearchResultType.Category,
                ForumId = forum.ForumId,
                ForumTitle = forum.Title,
                CategoryId = category.CategoryId,
                CategoryTitle = category.Title,
                ThreadId = null,
                ThreadTitle = null,
                PostId = null,
                CreatedBy = category.CreatedBy,
                CreatedAt = category.CreatedAt,
                Snippet = null,
                Rank = vector.Rank(search.TsQuery)
            };

        var threadResults =
            from thread in _dbContext.Threads
            join category in _dbContext.Categories on thread.CategoryId equals category.CategoryId
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(thread, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            where thread.CanReadThread(query.QueriedBy)
            select new SearchResultDto
            {
                Type = SearchResultType.Thread,
                ForumId = forum.ForumId,
                ForumTitle = forum.Title,
                CategoryId = category.CategoryId,
                CategoryTitle = category.Title,
                ThreadId = thread.ThreadId,
                ThreadTitle = thread.Title,
                PostId = null,
                CreatedBy = thread.CreatedBy,
                CreatedAt = thread.CreatedAt,
                Snippet = null,
                Rank = vector.Rank(search.TsQuery)
            };

        var postResults =
            from post in _dbContext.Posts
            join thread in _dbContext.Threads on post.ThreadId equals thread.ThreadId
            join category in _dbContext.Categories on thread.CategoryId equals category.CategoryId
            join forum in _dbContext.Forums on category.ForumId equals forum.ForumId
            from search in searchQuery
            let vector = Sql.Property<NpgsqlTsVector>(post, Constants.SearchVectorColumnName)
            where vector.Matches(search.TsQuery)
            where thread.CanReadThread(query.QueriedBy)
            select new SearchResultDto
            {
                Type = SearchResultType.Post,
                ForumId = forum.ForumId,
                ForumTitle = forum.Title,
                CategoryId = category.CategoryId,
                CategoryTitle = category.Title,
                ThreadId = thread.ThreadId,
                ThreadTitle = thread.Title,
                PostId = post.PostId,
                Snippet = PostgreSqlFullTextSearch.Headline(post.Content.ToSqlString(), search.TsQuery),
                CreatedBy = post.CreatedBy,
                CreatedAt = post.CreatedAt,
                Rank = vector.Rank(search.TsQuery)
            };

        var results = forumResults
            .Concat(categoryResults)
            .Concat(threadResults)
            .Concat(postResults)
            .AsCte("search_results");

        if (query.Type is { } type)
        {
            results = results.Where(result => result.Type == type);
        }

        results = ApplyCursor(results, query.Sort, cursor);

        var orderedResults = results.ApplySort(query);
        if (cursor is null && query.Offset != 0)
        {
            orderedResults = orderedResults.Skip(query.Offset.Value);
        }

        var rows = (await orderedResults
            .Take(query.Limit.Value + 1)
            .ToListAsyncLinqToDB(cancellationToken))
            .Select(NormalizeNullableIdentifiers)
            .ToList();

        var hasMore = rows.Count > query.Limit.Value;
        if (hasMore) rows.RemoveAt(query.Limit.Value);

        var last = rows.LastOrDefault();

        return new SearchResultsDto
        {
            Items = rows,
            NextCursor = hasMore && last != null
                ? CreateCursor(last)
                : null
        };
    }

    private static IQueryable<SearchResultDto> ApplyCursor(
        IQueryable<SearchResultDto> results,
        SortCriteria<SearchQuerySortType> sort,
        SearchCursorPayload? cursor)
    {
        if (cursor is null) return results;

        var cursorForumId = cursor.ForumId.Value;
        var cursorCategoryId = cursor.CategoryId is { } categoryId ? categoryId.Value : Guid.Empty;
        var cursorThreadId = cursor.ThreadId is { } threadId ? threadId.Value : Guid.Empty;
        var cursorPostId = cursor.PostId is { } postId ? postId.Value : Guid.Empty;

        return (sort.Field, sort.Order) switch
        {
            (SearchQuerySortType.Relevance, SortOrderType.Descending) => results.Where(result =>
                result.Rank < cursor.Rank ||
                (result.Rank == cursor.Rank &&
                 (result.CreatedAt < cursor.CreatedAt ||
                  (result.CreatedAt == cursor.CreatedAt &&
                   (result.Type < cursor.ResultType ||
                    (result.Type == cursor.ResultType &&
                     IsPastCursor(result, sort.Order, cursorForumId, cursorCategoryId, cursorThreadId, cursorPostId))))))),
            (SearchQuerySortType.Newest, SortOrderType.Descending) => results.Where(result =>
                result.CreatedAt < cursor.CreatedAt ||
                (result.CreatedAt == cursor.CreatedAt &&
                 (result.Type < cursor.ResultType ||
                  (result.Type == cursor.ResultType &&
                   IsPastCursor(result, sort.Order, cursorForumId, cursorCategoryId, cursorThreadId, cursorPostId))))),
            (SearchQuerySortType.Relevance, SortOrderType.Ascending) => results.Where(result =>
                result.Rank > cursor.Rank ||
                (result.Rank == cursor.Rank &&
                 (result.CreatedAt > cursor.CreatedAt ||
                  (result.CreatedAt == cursor.CreatedAt &&
                   (result.Type > cursor.ResultType ||
                    (result.Type == cursor.ResultType &&
                     IsPastCursor(result, sort.Order, cursorForumId, cursorCategoryId, cursorThreadId, cursorPostId))))))),
            (SearchQuerySortType.Newest, SortOrderType.Ascending) => results.Where(result =>
                result.CreatedAt > cursor.CreatedAt ||
                (result.CreatedAt == cursor.CreatedAt &&
                 (result.Type > cursor.ResultType ||
                  (result.Type == cursor.ResultType &&
                   IsPastCursor(result, sort.Order, cursorForumId, cursorCategoryId, cursorThreadId, cursorPostId))))),
            _ => throw new ArgumentOutOfRangeException(nameof(sort), sort, null)
        };
    }

    private static bool IsPrefixSearch(SearchTerm term) =>
        term.Value.Length >= 4 && term.Value.All(char.IsLetter);

    private static SearchResultDto NormalizeNullableIdentifiers(SearchResultDto row)
    {
        var hasCategory = row.Type is not SearchResultType.Forum;
        var hasThread = row.Type is SearchResultType.Thread or SearchResultType.Post;
        var hasPost = row.Type is SearchResultType.Post;

        return new SearchResultDto
        {
            Type = row.Type,
            ForumId = row.ForumId,
            ForumTitle = row.ForumTitle,
            CategoryId = hasCategory ? row.CategoryId : null,
            CategoryTitle = hasCategory ? row.CategoryTitle : null,
            ThreadId = hasThread ? row.ThreadId : null,
            ThreadTitle = hasThread ? row.ThreadTitle : null,
            PostId = hasPost ? row.PostId : null,
            Snippet = row.Snippet,
            CreatedBy = row.CreatedBy,
            CreatedAt = row.CreatedAt,
            Rank = row.Rank
        };
    }

    private static SearchCursor CreateCursor(SearchResultDto row) =>
        SearchCursor.From(SearchCursorPayload.Encode(row));

    private sealed class SearchQueryRow
    {
        public NpgsqlTsQuery TsQuery { get; init; } = null!;
    }

    [ExpressionMethod(nameof(IsPastCursorImpl))]
    private static bool IsPastCursor(
        SearchResultDto result,
        SortOrderType sortOrder,
        Guid cursorForumId,
        Guid cursorCategoryId,
        Guid cursorThreadId,
        Guid cursorPostId) =>
        throw new InvalidOperationException("This method should only be translated to SQL.");

    private static Expression<Func<SearchResultDto, SortOrderType, Guid, Guid, Guid, Guid, bool>> IsPastCursorImpl() =>
        (result, sortOrder, cursorForumId, cursorCategoryId, cursorThreadId, cursorPostId) =>
            (result.Type == SearchResultType.Forum &&
             IsInCursorOrder(result.ForumId.ToSqlGuid(), cursorForumId, sortOrder)) ||
            (result.Type == SearchResultType.Category &&
             (IsInCursorOrder(result.ForumId.ToSqlGuid(), cursorForumId, sortOrder) ||
              (result.ForumId.ToSqlGuid() == cursorForumId &&
               IsInCursorOrder(result.CategoryId!.Value.ToSqlGuid(), cursorCategoryId, sortOrder)))) ||
            (result.Type == SearchResultType.Thread &&
             (IsInCursorOrder(result.ForumId.ToSqlGuid(), cursorForumId, sortOrder) ||
              (result.ForumId.ToSqlGuid() == cursorForumId &&
               (IsInCursorOrder(result.CategoryId!.Value.ToSqlGuid(), cursorCategoryId, sortOrder) ||
                (result.CategoryId!.Value.ToSqlGuid() == cursorCategoryId &&
                 IsInCursorOrder(result.ThreadId!.Value.ToSqlGuid(), cursorThreadId, sortOrder)))))) ||
            (result.Type == SearchResultType.Post &&
             (IsInCursorOrder(result.ForumId.ToSqlGuid(), cursorForumId, sortOrder) ||
              (result.ForumId.ToSqlGuid() == cursorForumId &&
               (IsInCursorOrder(result.CategoryId!.Value.ToSqlGuid(), cursorCategoryId, sortOrder) ||
                (result.CategoryId!.Value.ToSqlGuid() == cursorCategoryId &&
                 (IsInCursorOrder(result.ThreadId!.Value.ToSqlGuid(), cursorThreadId, sortOrder) ||
                  (result.ThreadId!.Value.ToSqlGuid() == cursorThreadId &&
                   IsInCursorOrder(result.PostId!.Value.ToSqlGuid(), cursorPostId, sortOrder))))))));

    [ExpressionMethod(nameof(IsInCursorOrderImpl))]
    private static bool IsInCursorOrder(Guid value, Guid cursorValue, SortOrderType sortOrder) =>
        throw new InvalidOperationException("This method should only be translated to SQL.");

    private static Expression<Func<Guid, Guid, SortOrderType, bool>> IsInCursorOrderImpl() =>
        (value, cursorValue, sortOrder) =>
            sortOrder == SortOrderType.Descending
                ? Sql.Row(value) < Sql.Row(cursorValue)
                : Sql.Row(value) > Sql.Row(cursorValue);

    private sealed record SearchCursorPayload(
        float Rank,
        DateTime CreatedAt,
        SearchResultType ResultType,
        ForumId ForumId,
        CategoryId? CategoryId,
        ThreadId? ThreadId,
        PostId? PostId)
    {
        public static string Encode(SearchResultDto row)
        {
            var hasCategory = row.Type is not SearchResultType.Forum;
            var hasThread = row.Type is SearchResultType.Thread or SearchResultType.Post;
            var hasPost = row.Type is SearchResultType.Post;
            var payload = JsonSerializer.SerializeToUtf8Bytes(new SearchCursorPayload(
                row.Rank,
                row.CreatedAt,
                row.Type,
                row.ForumId,
                hasCategory ? row.CategoryId : null,
                hasThread ? row.ThreadId : null,
                hasPost ? row.PostId : null));

            return Convert.ToBase64String(payload)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        public static SearchCursorPayload? Decode(SearchCursor? cursor)
        {
            if (cursor is null) return null;

            try
            {
                var value = cursor.Value.Value
                    .Replace('-', '+')
                    .Replace('_', '/');
                value = value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');

                var payload = JsonSerializer.Deserialize<SearchCursorPayload>(Convert.FromBase64String(value));
                return payload is { IsValid: true } ? payload : null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private bool IsValid =>
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
