using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;
using Shared.Infrastructure.Persistence.Abstractions;
using Index = Shared.Domain.ValueObjects.Index;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetThreadPostsPagedQuery<>), typeof(Post))]
internal static partial class PostReadRepositoryExtensions
{
    [SortExpression<GetThreadPostsPagedQuerySortType>(GetThreadPostsPagedQuerySortType.Index)]
    private static readonly Expression<Func<Post, object>> IndexExpression = e => new { e.CreatedAt, e.PostId };
}

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PostReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, PostNotFoundError, PermissionDeniedError>> GetOneAsync<T>(GetPostQuery<T> query,
        CancellationToken cancellationToken) where T : notnull
    {
        var result = await (
                from p in _dbContext.Posts
                from t in _dbContext.Threads
                    .Where(e => e.ThreadId == p.ThreadId)
                where p.PostId == query.PostId
                select new ProjectionWithAccess<Post>
                {
                    Projection = p,
                    HasAccess = t.CanReadThread(query.QueriedBy)
                }
            )
            .ProjectToType<ProjectionWithAccess<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError();
        if (!result.HasAccess) return new PermissionDeniedError();

        return result.Projection;
    }

    public async Task<Result<IReadOnlyList<T>, ThreadNotFoundError, PermissionDeniedError>> GetThreadPostsAsync<T>(
        GetThreadPostsPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        var postsQuery = _dbContext.Posts
            .Where(e => e.ThreadId == request.ThreadId)
            .ApplySort(request)
            .ApplyPagination(request);

        var projections = await (
                from thread in _dbContext.Threads.Where(e => e.ThreadId == request.ThreadId)
                let canRead = thread.CanReadThread(request.QueriedBy)
                from post in postsQuery
                    .Where(_ => canRead)
                    .DefaultIfEmpty()
                select new SqlKeyValue<bool, Post?>
                {
                    Key = canRead,
                    Value = post
                })
            .ProjectToType<SqlKeyValue<bool, T?>>()
            .ToListAsyncLinqToDB(cancellationToken);

        if (projections.Count == 0) return new ThreadNotFoundError();
        if (!projections[0].Key) return new PermissionDeniedError();

        return projections
            .Where(e => e.Value is not null)
            .Select(e => e.Value!)
            .ToList();
    }

    public async Task<Result<Index, PostNotFoundError, PermissionDeniedError>> GetPostIndexAsync(
        GetPostIndexQuery query,
        CancellationToken cancellationToken)
    {
        var result = await (
                from p in _dbContext.Posts
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                where p.PostId == query.PostId
                select new ProjectionWithAccess<int>
                {
                    Projection = _dbContext.Posts.Count(e =>
                        e.ThreadId == p.ThreadId && Sql.Row(e.CreatedAt, e.PostId) < Sql.Row(p.CreatedAt, p.PostId)),
                    HasAccess = t.CanReadThread(query.QueriedBy)
                }
            )
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError();
        if (!result.HasAccess) return new PermissionDeniedError();

        return Index.From(result.Projection);
    }
}
