using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;
using static Shared.Infrastructure.Extensions.QueryableExtensions;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class AccessReadRepository : IAccessReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public AccessReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private Task<Dictionary<PolicyType, bool>> GetPermissions(IQueryable<Policy[]> queryable, UserId? userId,
        CancellationToken cancellationToken)
    {
        if (userId == null)
        {
            return queryable
                .SelectMany(e => e.Select(x => new { Key = x.Type, Value = x.Value == PolicyValue.Any }))
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return queryable
            .SelectMany(e => e.Select(x => new
            {
                Key = x.Type,
                Value = x.Value != PolicyValue.Granted ||
                        _dbContext.Grants.Any(y => y.PolicyId == x.PolicyId && y.UserId == userId.Value)
            }))
            .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    private Task<Dictionary<PolicyType, bool>> GetPermissions(IQueryable<Policy> queryable, UserId? userId,
        CancellationToken cancellationToken)
    {
        if (userId == null)
        {
            return queryable
                .Select(e => new { Key = e.Type, Value = e.Value == PolicyValue.Any })
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return queryable
            .Select(e => new
            {
                Key = e.Type,
                Value = e.Value != PolicyValue.Granted ||
                        _dbContext.Grants.Any(y => y.PolicyId == e.PolicyId && y.UserId == userId.Value)
            })
            .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    public async Task<Dictionary<PolicyType, bool>> GetPortalPermissionsAsync(GetPortalPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Portal.Select(e => new Policy[]
        {
            e.ReadPolicy, e.ForumCreatePolicy, e.CategoryCreatePolicy, e.ThreadCreatePolicy, e.PostCreatePolicy
        });
        return await GetPermissions(queryable, query.QueriedBy, cancellationToken);
    }

    public async Task<Result<Dictionary<PolicyType, bool>, ForumNotFoundError>> GetForumPermissionsAsync(
        GetForumPermissionsQuery query, CancellationToken cancellationToken)
    {
        var queryable =
            from f in _dbContext.Forums.Where(e => e.ForumId == query.ForumId)
            from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                SqlArray(f.ReadPolicyId, f.CategoryCreatePolicyId, f.ThreadCreatePolicyId, f.PostCreatePolicyId)))
            select p;

        var result = await GetPermissions(queryable, query.QueriedBy, cancellationToken);

        return result.Count == 0 ? new ForumNotFoundError(query.ForumId) : result;
    }
}