using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Infrastructure.Extensions;
using UserService.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class AccessRestrictionReadRepository : IAccessRestrictionReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public AccessRestrictionReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<Result<Success, ForumNotFoundError, PolicyViolationError, AccessPolicyRestrictedError,
            CategoryCreatePolicyRestrictedError>>
        CheckUserCanCreateCategoryAsync(UserId? userId, ForumId forumId, DateTime timestamp,
            CancellationToken cancellationToken)
    {
        var result = await (
                from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == f.AccessPolicyId)
                from cp in _dbContext.Policies.Where(e => e.PolicyId == f.CategoryCreatePolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.PolicyId == f.AccessPolicyId)
                    .DefaultIfEmpty()
                from cg in _dbContext.Grants
                    .Where(e => e.PolicyId == f.CategoryCreatePolicyId)
                    .DefaultIfEmpty()
                select new
                {
                    Policies = new
                    {
                        Access = new { ap.PolicyId, ap.Value },
                        Create = new { cp.PolicyId, cp.Value }
                    },
                    Grants = new
                    {
                        Access = userId == null || ag.PolicyId.SqlIsNotNull(),
                        Create = userId == null || cg.PolicyId.SqlIsNotNull()
                    },
                    Restrictions = userId == null
                        ? null
                        : (
                            from r in _dbContext.ForumRestrictions
                            where r.UserId == userId && r.ForumId == f.ForumId &&
                                  (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                            group r by new { r.UserId, r.ForumId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault()
                })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ForumNotFoundError(forumId);

        if ((result.Policies.Access.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Access.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Access)
                return new PolicyViolationError(result.Policies.Access.PolicyId, userId);
        }

        if ((result.Policies.Create.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Create.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Create)
                return new PolicyViolationError(result.Policies.Create.PolicyId, userId);
        }

        if (userId != null && result.Restrictions != null)
        {
            if (result.Restrictions.Contains((byte)PolicyType.Access)) return new AccessPolicyRestrictedError(userId);
            if (result.Restrictions.Contains((byte)PolicyType.CategoryCreate))
                return new CategoryCreatePolicyRestrictedError(userId);
        }

        return Success.Instance;
    }

    public async
        Task<Result<Success, CategoryNotFoundError, PolicyViolationError, AccessPolicyRestrictedError,
            ThreadCreatePolicyRestrictedError>>
        CanUserCanCreateThreadAsync(UserId? userId, CategoryId categoryId, DateTime timestamp,
            CancellationToken cancellationToken)
    {
        var result = await (
                from c in _dbContext.Categories.Where(e => e.CategoryId == categoryId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == c.AccessPolicyId)
                from cp in _dbContext.Policies.Where(e => e.PolicyId == c.ThreadCreatePolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.PolicyId == c.AccessPolicyId)
                    .DefaultIfEmpty()
                from cg in _dbContext.Grants
                    .Where(e => e.PolicyId == c.ThreadCreatePolicyId)
                    .DefaultIfEmpty()
                select new
                {
                    Policies = new
                    {
                        Access = new { ap.PolicyId, ap.Value },
                        Create = new { cp.PolicyId, cp.Value }
                    },
                    Grants = new
                    {
                        Access = userId == null || ag.PolicyId.SqlIsNotNull(),
                        Create = userId == null || cg.PolicyId.SqlIsNotNull()
                    },
                    Restrictions = userId == null
                        ? null
                        : (
                            from r in (
                                    from r in _dbContext.ForumRestrictions
                                    where r.UserId == userId && r.ForumId == c.ForumId &&
                                          (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                                    select new { UserId = userId, Policy = (short)r.Policy }
                                )
                                .Concat(
                                    from r in _dbContext.CategoryRestrictions
                                    where r.UserId == userId && r.CategoryId == c.CategoryId &&
                                          (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                                    select new { UserId = userId, Policy = (short)r.Policy }
                                )
                            group r by r.UserId
                            into g
                            select g.ArrayAggregate(e => e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault()
                })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new CategoryNotFoundError(categoryId);

        if ((result.Policies.Access.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Access.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Access)
                return new PolicyViolationError(result.Policies.Access.PolicyId, userId);
        }

        if ((result.Policies.Create.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Create.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Create)
                return new PolicyViolationError(result.Policies.Create.PolicyId, userId);
        }

        if (userId != null && result.Restrictions != null)
        {
            if (result.Restrictions.Contains((byte)PolicyType.Access)) return new AccessPolicyRestrictedError(userId);
            if (result.Restrictions.Contains((byte)PolicyType.CategoryCreate))
                return new ThreadCreatePolicyRestrictedError(userId);
        }

        return Success.Instance;
    }

    public async Task<Result<Success, ThreadNotFoundError, PolicyViolationError, AccessPolicyRestrictedError,
            PostCreatePolicyRestrictedError>>
        CheckUserCanCreatePostAsync(UserId? userId, ThreadId threadId, DateTime timestamp,
            CancellationToken cancellationToken)
    {
        var result = await (
                from t in _dbContext.Threads.Where(e => e.ThreadId == threadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from ap in _dbContext.Policies.Where(e => e.PolicyId == t.AccessPolicyId)
                from cp in _dbContext.Policies.Where(e => e.PolicyId == t.PostCreatePolicyId)
                from ag in _dbContext.Grants
                    .Where(e => e.PolicyId == t.AccessPolicyId)
                    .DefaultIfEmpty()
                from cg in _dbContext.Grants
                    .Where(e => e.PolicyId == t.PostCreatePolicyId)
                    .DefaultIfEmpty()
                select new
                {
                    Policies = new
                    {
                        Access = new { ap.PolicyId, ap.Value },
                        Create = new { cp.PolicyId, cp.Value }
                    },
                    Grants = new
                    {
                        Access = userId == null || ag.PolicyId.SqlIsNotNull(),
                        Create = userId == null || cg.PolicyId.SqlIsNotNull()
                    },
                    Restrictions = userId == null
                        ? null
                        : (
                            from r in (
                                    from r in _dbContext.ForumRestrictions
                                    where r.UserId == userId && r.ForumId == c.ForumId &&
                                          (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                                    select new { UserId = userId, Policy = (short)r.Policy }
                                )
                                .Concat(
                                    from r in _dbContext.CategoryRestrictions
                                    where r.UserId == userId && r.CategoryId == t.CategoryId &&
                                          (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                                    select new { UserId = userId, Policy = (short)r.Policy }
                                )
                                .Concat(
                                    from r in _dbContext.ThreadRestrictions
                                    where r.UserId == userId && r.ThreadId == t.ThreadId &&
                                          (r.ExpiredAt == null || r.ExpiredAt > timestamp)
                                    select new { UserId = userId, Policy = (short)r.Policy }
                                )
                            group r by r.UserId
                            into g
                            select g.ArrayAggregate(e => e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault()
                })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ThreadNotFoundError(threadId);

        if ((result.Policies.Access.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Access.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Access)
                return new PolicyViolationError(result.Policies.Access.PolicyId, userId);
        }

        if ((result.Policies.Create.Value > PolicyValue.Any && userId == null) ||
            result.Policies.Create.Value == PolicyValue.Granted)
        {
            if (!result.Grants.Create)
                return new PolicyViolationError(result.Policies.Create.PolicyId, userId);
        }

        if (userId != null && result.Restrictions != null)
        {
            if (result.Restrictions.Contains((byte)PolicyType.Access)) return new AccessPolicyRestrictedError(userId);
            if (result.Restrictions.Contains((byte)PolicyType.CategoryCreate))
                return new PostCreatePolicyRestrictedError(userId);
        }

        return Success.Instance;
    }
}