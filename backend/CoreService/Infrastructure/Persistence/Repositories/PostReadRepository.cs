using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

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

    public async Task<Result<T, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>> GetOneAsync<T>(
        GetPostQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await (
                from p in _dbContext.Posts
                from t in _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                    .Where(e => e.Projection.ThreadId == p.ThreadId)
                where p.PostId == query.PostId
                select new ProjectionWithAccessInfo<Post>
                {
                    Projection = p,
                    ReadPolicyId = t.ReadPolicyId,
                    ReadPolicyValue = t.ReadPolicyValue,
                    HasGrant = t.HasGrant,
                    HasRestriction = t.HasRestriction
                }
            )
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError(query.PostId);

        if ((result.ReadPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.ReadPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.ReadPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new PolicyRestrictedError(PolicyType.Read, query.QueriedBy);

        return result.Projection;
    }

    public async Task<Result<IReadOnlyList<T>, ThreadNotFoundError>> GetThreadPostsAsync<T>(
        GetThreadPostsPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        if (!await _dbContext.Threads.AnyAsyncLinqToDB(e => e.ThreadId == request.ThreadId, cancellationToken))
            return new ThreadNotFoundError(request.ThreadId);

        var query = _dbContext.Posts
            .Where(e => e.ThreadId == request.ThreadId)
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>();

        var result = await query.ToListAsyncLinqToDB(cancellationToken);

        return result;
    }

    public async Task<Result<PostIndex, PostNotFoundError, PolicyViolationError, PolicyRestrictedError>>
        GetPostIndexAsync(GetPostIndexQuery query, CancellationToken cancellationToken)
    {
        var result = await (
                from p in _dbContext.Posts
                from t in _dbContext.GetThreadsWithAccessInfo(query.QueriedBy)
                    .Where(e => e.Projection.ThreadId == p.ThreadId)
                where p.PostId == query.PostId
                select new ProjectionWithAccessInfo<long>
                {
                    Projection = _dbContext.Posts.LongCount(e =>
                        e.ThreadId == p.ThreadId && Sql.Row(e.CreatedAt, e.PostId) < Sql.Row(p.CreatedAt, p.PostId)),
                    ReadPolicyId = t.ReadPolicyId,
                    ReadPolicyValue = t.ReadPolicyValue,
                    HasGrant = t.HasGrant,
                    HasRestriction = t.HasRestriction
                }
            )
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError(query.PostId);

        if ((result.ReadPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.ReadPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.ReadPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new PolicyRestrictedError(PolicyType.Read, query.QueriedBy);

        return PostIndex.From((ulong)result.Projection);
    }
}