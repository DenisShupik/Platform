using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using LinqToDB.EntityFrameworkCore;
using UserService.Domain.ValueObjects;

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

    public async Task<Dictionary<PolicyType, bool>> GetPortalPermissionsAsync(GetUserPortalPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Portal.Select(e => new Policy[]
        {
            e.ReadPolicy, e.ForumCreatePolicy, e.CategoryCreatePolicy, e.ThreadCreatePolicy, e.PostCreatePolicy
        });
        return await GetPermissions(queryable, query.QueriedBy, cancellationToken);
    }
}