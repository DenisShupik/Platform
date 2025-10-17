using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Persistence.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;
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

    public async Task<Dictionary<PolicyType, bool>> GetPortalPermissionsAsync(GetPortalPermissionsQuery query,
        CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Portal.Select(e => new Policy[]
        {
            e.ReadPolicy, e.ForumCreatePolicy, e.CategoryCreatePolicy, e.ThreadCreatePolicy, e.PostCreatePolicy
        });

        if (query.QueriedBy == null)
        {
            return await queryable
                .SelectMany(e => e.Select(x => new { Key = x.Type, Value = x.Value == PolicyValue.Any }))
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return await queryable
            .SelectMany(e => e.Select(x => new
            {
                Key = x.Type,
                Value = _dbContext.IsPolicySatisfied(x, query.QueriedBy.Value)
            }))
            .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
    }

    public async Task<Result<Dictionary<PolicyType, bool>, ForumNotFoundError>> GetForumPermissionsAsync(
        GetForumPermissionsQuery query, CancellationToken cancellationToken)
    {
        Dictionary<PolicyType, bool> result;
        if (query.QueriedBy == null)
        {
            result = await (
                    from f in _dbContext.Forums.Where(e => e.ForumId == query.ForumId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(f.ReadPolicyId, f.CategoryCreatePolicyId, f.ThreadCreatePolicyId,
                            f.PostCreatePolicyId)))
                    select new { Key = p.Type, Value = p.Value == PolicyValue.Any }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }
        else
        {
            var userId = query.QueriedBy.Value;
            var timestamp = query.EvaluatedAt;
            result = await (
                    from f in _dbContext.Forums.Where(e => e.ForumId == query.ForumId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(f.ReadPolicyId, f.CategoryCreatePolicyId, f.ThreadCreatePolicyId,
                            f.PostCreatePolicyId)))
                    from fr in _dbContext.GetForumRestriction(userId, f.ForumId, p.Type, timestamp)
                    select new
                    {
                        Key = p.Type,
                        Value = fr.Type.SqlIsNull() &&
                                _dbContext.IsPolicySatisfied(p, userId)
                    }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return result.Count == 0 ? new ForumNotFoundError(query.ForumId) : result;
    }

    public async Task<Result<Dictionary<PolicyType, bool>, CategoryNotFoundError>> GetCategoryPermissionsAsync(
        GetCategoryPermissionsQuery query, CancellationToken cancellationToken)
    {
        Dictionary<PolicyType, bool> result;
        if (query.QueriedBy == null)
        {
            result = await (
                    from c in _dbContext.Categories.Where(e => e.CategoryId == query.CategoryId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(c.ReadPolicyId, c.ThreadCreatePolicyId, c.PostCreatePolicyId)))
                    select new { Key = p.Type, Value = p.Value == PolicyValue.Any }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }
        else
        {
            var userId = query.QueriedBy.Value;
            var timestamp = query.EvaluatedAt;
            result = await (
                    from c in _dbContext.Categories.Where(e => e.CategoryId == query.CategoryId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(c.ReadPolicyId, c.ThreadCreatePolicyId, c.PostCreatePolicyId)))
                    from cr in _dbContext.GetCategoryRestriction(userId, c.CategoryId, p.Type, timestamp)
                    from fr in _dbContext.GetForumRestriction(userId, c.ForumId, p.Type, timestamp)
                    select new
                    {
                        Key = p.Type,
                        Value = cr.Type.SqlIsNull() &&
                                fr.Type.SqlIsNull() &&
                                _dbContext.IsPolicySatisfied(p, userId)
                    }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return result.Count == 0 ? new CategoryNotFoundError(query.CategoryId) : result;
    }

    public async Task<Result<Dictionary<PolicyType, bool>, ThreadNotFoundError>> GetThreadPermissionsAsync(
        GetThreadPermissionsQuery query, CancellationToken cancellationToken)
    {
        Dictionary<PolicyType, bool> result;
        if (query.QueriedBy == null)
        {
            result = await (
                    from t in _dbContext.Threads.Where(e => e.ThreadId == query.ThreadId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(t.ReadPolicyId, t.PostCreatePolicyId)))
                    select new { Key = p.Type, Value = p.Value == PolicyValue.Any }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }
        else
        {
            var userId = query.QueriedBy.Value;
            var timestamp = query.EvaluatedAt;
            result = await (
                    from t in _dbContext.Threads.Where(e => e.ThreadId == query.ThreadId)
                    from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                    from p in _dbContext.Policies.Where(e => Sql.Ext.PostgreSQL().ValueIsEqualToAny(e.PolicyId,
                        SqlArray(t.ReadPolicyId, t.PostCreatePolicyId)))
                    from tr in _dbContext.GetThreadRestriction(userId, t.ThreadId, p.Type, timestamp)
                    from cr in _dbContext.GetCategoryRestriction(userId, t.CategoryId, p.Type, timestamp)
                    from fr in _dbContext.GetForumRestriction(userId, c.ForumId, p.Type, timestamp)
                    select new
                    {
                        Key = p.Type,
                        Value = tr.Type.SqlIsNull() &&
                                cr.Type.SqlIsNull() &&
                                fr.Type.SqlIsNull() &&
                                _dbContext.IsPolicySatisfied(p, userId)
                    }
                )
                .ToDictionaryAsyncLinqToDB(e => e.Key, e => e.Value, cancellationToken);
        }

        return result.Count == 0 ? new ThreadNotFoundError(query.ThreadId) : result;
    }

    public async Task<Result<Success, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>>
        EvaluatedForumPolicy(ForumId forumId, UserId? userId, PolicyType type, DateTime evaluatedAt,
            CancellationToken cancellationToken)
    {
        if (userId == null)
        {
            var result = await (
                    from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
                    from p in _dbContext.Policies.Where(e => e.PolicyId == (
                        type == PolicyType.Read ? f.ReadPolicyId :
                        type == PolicyType.CategoryCreate ? f.CategoryCreatePolicyId :
                        type == PolicyType.ThreadCreate ? f.ThreadCreatePolicyId : f.PostCreatePolicyId))
                    select new
                    {
                        p.PolicyId,
                        IsPolicySatisfied = p.Value == PolicyValue.Any
                    }
                )
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);

            if (result == null) return new ForumNotFoundError(forumId);
            if (!result.IsPolicySatisfied) return new PolicyViolationError(result.PolicyId, userId);
        }
        else
        {
            var result = await (
                    from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
                    from p in _dbContext.Policies.Where(e => e.PolicyId == (
                        type == PolicyType.Read ? f.ReadPolicyId :
                        type == PolicyType.CategoryCreate ? f.CategoryCreatePolicyId :
                        type == PolicyType.ThreadCreate ? f.ThreadCreatePolicyId : f.PostCreatePolicyId))
                    from fr in _dbContext.GetForumRestriction(userId.Value, f.ForumId, p.Type, evaluatedAt)
                    select new
                    {
                        p.PolicyId,
                        IsPolicySatisfied = _dbContext.IsPolicySatisfied(p, userId.Value),
                        HasRestriction = fr.Type.SqlIsNotNull()
                    }
                )
                .FirstOrDefaultAsyncLinqToDB(cancellationToken);

            if (result == null) return new ForumNotFoundError(forumId);
            if (!result.IsPolicySatisfied) return new PolicyViolationError(result.PolicyId, userId);
            if (result.HasRestriction) return new PolicyRestrictedError(userId);
        }

        return Success.Instance;
    }
}