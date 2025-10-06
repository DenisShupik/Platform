using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using Shared.Infrastructure.Generator;

namespace CoreService.Infrastructure.Persistence.Repositories;

[GenerateApplySort(typeof(GetForumsPagedQuery<>), typeof(Forum))]
internal static partial class ForumReadRepositoryExtensions;

public sealed class ForumReadRepository : IForumReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public ForumReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<T, ForumNotFoundError, PolicyViolationError, AccessPolicyRestrictedError>> GetOneAsync<T>(
        GetForumQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var timestamp = DateTimeOffset.UtcNow;
        var result = await (
                from f in _dbContext.Categories.Where(e => e.ForumId == query.ForumId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == f.AccessPolicyId)
                select new
                {
                    Projection = f,
                    AccessPolicyId = ap.PolicyId,
                    AccessPolicyValue = ap.Value,
                    HasGrant = query.QueriedBy == null || (
                            from ag in _dbContext.Grants
                            where ag.PolicyId == f.AccessPolicyId
                            select ag.PolicyId
                        )
                        .FirstOrDefault()
                        .SqlIsNotNull(),
                    HasRestriction = query.QueriedBy != null && (
                        (
                            from r in _dbContext.ForumRestrictions
                            where r.UserId == query.QueriedBy &&
                                  r.ForumId == f.ForumId &&
                                  r.Policy == PolicyType.Access &&
                                  (r.ExpiredAt == null ||
                                   r.ExpiredAt.Value > timestamp)
                            select r
                        ).Any()
                    )
                })
            .ProjectToType<GetProjection<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ForumNotFoundError(query.ForumId);

        if ((result.AccessPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.AccessPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.AccessPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new AccessPolicyRestrictedError(query.QueriedBy);

        return result.Projection;
    }

    public async Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ForumId, Guid> ids, CancellationToken cancellationToken)
    {
        var projection = await _dbContext.Forums
            .Where(e => ids.ToHashSet().Contains(e.ForumId))
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> request,
        CancellationToken cancellationToken)
    {
        IQueryable<Forum> query = _dbContext.Forums;

        if (request.Title != null)
        {
            query = query.Where(e =>
                e.Title.ToSqlString().Contains(request.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        if (request.CreatedBy != null)
        {
            query = query.Where(e => e.CreatedBy == request.CreatedBy.Value);
        }

        var forums = await query
            .ApplySort(request)
            .ApplyPagination(request)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<ulong> GetCountAsync(GetForumsCountQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Forums
            .Where(e => request.CreatedBy == null || e.CreatedBy == request.CreatedBy);

        var count = await query.LongCountAsyncLinqToDB(cancellationToken);

        return (ulong)count;
    }

    public async Task<Dictionary<ForumId, ulong>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken)
    {
        var forums = request.ForumIds.Select(x => x.Value).ToArray();
        var query =
            from f in _dbContext.Forums
            from c in f.Categories
            where Sql.Ext.PostgreSQL().ValueIsEqualToAny(f.ForumId, forums)
            group c by f.ForumId
            into g
            select new { g.Key, Value = g.LongCount() };

        return await query.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);
    }
}