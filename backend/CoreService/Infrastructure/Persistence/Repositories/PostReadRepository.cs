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
        var timestamp = DateTimeOffset.UtcNow;
        var result = await (
                from p in _dbContext.Posts.Where(e => e.PostId == query.PostId)
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == c.AccessPolicyId)
                select new
                {
                    Projection = p,
                    AccessPolicyId = ap.PolicyId,
                    AccessPolicyValue = ap.Value,
                    HasGrant = query.QueriedBy == null || (
                            from ag in _dbContext.Grants
                            where ag.PolicyId == c.AccessPolicyId
                            select ag.PolicyId
                        )
                        .FirstOrDefault()
                        .SqlIsNotNull(),
                    HasRestriction = query.QueriedBy != null && (
                        (
                            from r in _dbContext.ForumRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.ForumId == c.ForumId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        ).Any() ||
                        (
                            from r in _dbContext.CategoryRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.CategoryId == t.CategoryId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        )
                        .Any() ||
                        (
                            from r in _dbContext.ThreadRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.ThreadId == p.ThreadId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        )
                        .Any()
                    )
                })
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError(query.PostId);

        if ((result.AccessPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.AccessPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.AccessPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new AccessPolicyRestrictedError(query.QueriedBy);

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

    public async Task<Result<PostIndex, PostNotFoundError, PolicyViolationError, AccessPolicyRestrictedError>>
        GetPostIndexAsync(GetPostIndexQuery query, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow;
        var result = await (
                from p in _dbContext.Posts.Where(e => e.PostId == query.PostId)
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == c.AccessPolicyId)
                select new
                {
                    Projection = _dbContext.Posts.LongCount(e =>
                        e.ThreadId == p.ThreadId && Sql.Row(e.CreatedAt, e.PostId) < Sql.Row(p.CreatedAt, p.PostId)),
                    AccessPolicyId = ap.PolicyId,
                    AccessPolicyValue = ap.Value,
                    HasGrant = query.QueriedBy == null || (
                            from ag in _dbContext.Grants
                            where ag.PolicyId == c.AccessPolicyId
                            select ag.PolicyId
                        )
                        .FirstOrDefault()
                        .SqlIsNotNull(),
                    HasRestriction = query.QueriedBy != null && (
                        (
                            from r in _dbContext.ThreadRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.ThreadId == p.ThreadId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        )
                        .Any() ||
                        (
                            from r in _dbContext.CategoryRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.CategoryId == t.CategoryId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        )
                        .Any() || (
                            from r in _dbContext.ForumRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.ForumId == c.ForumId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        ).Any()
                    )
                })
            .ProjectToType<ProjectionWithAccessInfo<long>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new PostNotFoundError(query.PostId);

        if ((result.AccessPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.AccessPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.AccessPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new AccessPolicyRestrictedError(query.QueriedBy);

        return PostIndex.From((ulong)result.Projection);
    }
}