using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Abstractions;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
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

    public async Task<Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>> GetOneAsync<T>(
        GetForumQuery<T> query, CancellationToken cancellationToken)
        where T : notnull
    {
        var result = await _dbContext.GetForumsWithAccessInfo(query.QueriedBy)
            .Where(e => e.Projection.ForumId == query.ForumId)
            .ProjectToType<ProjectionWithAccessInfo<T>>()
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ForumNotFoundError(query.ForumId);

        if ((result.ReadPolicyValue > PolicyValue.Any && query.QueriedBy == null) ||
            result.ReadPolicyValue == PolicyValue.Granted)
        {
            if (!result.HasGrant)
                return new PolicyViolationError(result.ReadPolicyId, query.QueriedBy);
        }

        if (result.HasRestriction) return new PolicyRestrictedError(PolicyType.Read, query.QueriedBy);

        return result.Projection;
    }

    public async Task<Dictionary<ForumId,
            Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>>>
        GetBulkAsync<T>(GetForumsBulkQuery<T> query,
            CancellationToken cancellationToken)
        where T : notnull
    {
        var ids = query.ForumIds.Select(x => x.Value).ToArray();
        var projection = await (
                from id in _dbContext.ToTvcLinqToDb(ids)
                from p in _dbContext.GetForumsWithAccessInfo(query.QueriedBy)
                    .Where(e => e.Projection.ForumId == id)
                    .DefaultIfEmpty()
                select new SqlKeyValue<Guid, ProjectionWithAccessInfo<Forum>?>
                {
                    Key = id,
                    Value = p
                })
            .ProjectToType<SqlKeyValue<Guid, ProjectionWithAccessInfo<T>?>>()
            .ToDictionaryAsyncLinqToDB(k => ForumId.From(k.Key),
                k => (Result<T, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>)(k.Value == null
                    ? new ForumNotFoundError(ForumId.From(k.Key))
                    : k.Value.Projection), cancellationToken);

        return projection;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetForumsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.GetForumsWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection);

        if (query.Title != null)
        {
            queryable = queryable.Where(e =>
                e.Title.ToSqlString()
                    .Contains(query.Title.Value.Value, StringComparison.CurrentCultureIgnoreCase));
        }

        if (query.CreatedBy != null)
        {
            queryable = queryable.Where(e => e.CreatedBy == query.CreatedBy.Value);
        }

        var forums = await queryable
            .ApplySort(query)
            .ApplyPagination(query)
            .ProjectToType<T>()
            .ToListAsyncLinqToDB(cancellationToken);

        return forums;
    }

    public async Task<ulong> GetCountAsync(GetForumsCountQuery query, CancellationToken cancellationToken)
    {
        var queryable = _dbContext.GetForumsWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection)
            .Where(e => query.CreatedBy == null || e.CreatedBy == query.CreatedBy);

        var count = await queryable.LongCountAsyncLinqToDB(cancellationToken);

        return (ulong)count;
    }

    public async Task<Dictionary<ForumId, ulong>> GetForumsCategoriesCountAsync(GetForumsCategoriesCountQuery query,
        CancellationToken cancellationToken)
    {
        var ids = query.ForumIds.Select(x => x.Value).ToArray();

        var queryable = _dbContext.GetCategoriesWithAccessInfo(query.QueriedBy)
            .OnlyAvailable(query.QueriedBy)
            .Select(e => e.Projection)
            .Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.ForumId, ids))
            .GroupBy(e => e.ForumId)
            .Select(e => new { e.Key, Value = e.LongCount() });

        return await queryable.ToDictionaryAsyncLinqToDB(e => e.Key, e => (ulong)e.Value, cancellationToken);
    }
}