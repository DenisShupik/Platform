using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using System.Linq.Expressions;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetBookmarkedPostsPagedQuery<>), typeof(PostBookmark))]
internal static partial class PostBookmarkReadRepositoryExtensions
{
    [SortExpression<GetBookmarkedPostsPagedQuerySortType>(GetBookmarkedPostsPagedQuerySortType.CreatedAt)]
    private static readonly Expression<Func<PostBookmark, object>> CreatedAtExpression =
        e => new { e.CreatedAt, e.PostId };
}

public sealed class PostBookmarkReadRepository : IPostBookmarkReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PostBookmarkReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(UserId userId, PostId postId, CancellationToken cancellationToken)
    {
        return _dbContext.PostBookmarks
            .AnyAsyncLinqToDB(e => e.UserId == userId && e.PostId == postId, cancellationToken);
    }

    public async Task<IReadOnlyList<PostId>> GetBookmarkedPostIdsAsync(
        UserId userId,
        IdSet<PostId, Guid> postIds,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.PostBookmarks
            .Where(e => e.UserId == userId && postIds.Contains(e.PostId))
            .Select(e => e.PostId)
            .ToListAsyncLinqToDB(cancellationToken);
    }

    public async Task<List<T>> GetBookmarkedPostsAsync<T>(
        GetBookmarkedPostsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        var bookmarks = GetAccessibleBookmarks(query.UserId, query.RequestedBy);

        return await (
                from bookmark in bookmarks.ApplySort(query).ApplyPagination(query)
                join post in _dbContext.Posts on bookmark.PostId equals post.PostId
                select post
            )
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);
    }

    public async Task<Count> GetBookmarkedPostsCountAsync(
        GetBookmarkedPostsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        var count = await GetAccessibleBookmarks(query.UserId, query.RequestedBy).CountAsyncLinqToDB(cancellationToken);
        return Count.From(count);
    }

    private IQueryable<PostBookmark> GetAccessibleBookmarks(UserId userId, UserIdRole requestedBy)
    {
        return
            from bookmark in _dbContext.PostBookmarks
            join post in _dbContext.Posts on bookmark.PostId equals post.PostId
            join thread in _dbContext.Threads on post.ThreadId equals thread.ThreadId
            where bookmark.UserId == userId && thread.CanReadThread(requestedBy)
            select bookmark;
    }
}
