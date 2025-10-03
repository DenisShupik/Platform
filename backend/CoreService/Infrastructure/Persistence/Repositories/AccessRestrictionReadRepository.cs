using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
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
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class AccessRestrictionReadRepository : IAccessRestrictionReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public AccessRestrictionReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Success, ForumAccessPolicyViolationError, ForumPolicyRestrictedError>>
        CheckUserAccessAsync(
            UserId? userId, ForumId forumId, CancellationToken cancellationToken)
    {
        // var cte = (
        //         from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
        //         select new
        //         {
        //             Forum = new { f.ForumId, AccessPolicy = f.GetForumAccessPolicy() }
        //         }
        //     )
        //     .AsCte();
        //
        // var queryable =
        //     from ar in _dbContext.ForumAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
        //     select new { ar.RestrictionLevel };
        //
        // var queryable2 =
        //     from c in cte
        //     select new
        //     {
        //         AccessLevels = c,
        //         Restriction = userId == null ? null : queryable.FirstOrDefault()
        //     };
        //
        //
        // var result = await queryable2.FirstAsyncLinqToDB(cancellationToken);
        //
        // var accessLevels = result.AccessLevels;
        // var restriction = result.Restriction;
        //
        // if (userId == null)
        // {
        //     if (accessLevels.Forum.AccessLevel > AccessLevel.Public)
        //         return new ForumAccessPolicyViolationError(accessLevels.Forum.ForumId, userId,
        //             accessLevels.Forum.AccessLevel);
        // }
        // else
        // {
        //     if (restriction != null)
        //     {
        //         return new ForumAccessRestrictedError(accessLevels.Forum.ForumId, userId.Value,
        //             restriction.RestrictionLevel);
        //     }
        // }

        return Success.Instance;
    }

    public async
        Task<Result<Success, ForumAccessPolicyViolationError, CategoryAccessPolicyViolationError,
            ForumPolicyRestrictedError,
            CategoryPolicyRestrictedError>> CheckUserAccessAsync(UserId? userId, CategoryId categoryId,
            CancellationToken cancellationToken)
    {
        // var cte = (
        //         from c in _dbContext.Categories.Where(e => e.CategoryId == categoryId)
        //         from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
        //         select new
        //         {
        //             Forum = new { f.ForumId, f.AccessLevel },
        //             Category = new { c.CategoryId, c.AccessLevel }
        //         }
        //     )
        //     .AsCte();
        //
        // var queryable =
        //     from ar in _dbContext.ForumAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
        //     select new { Forum = (RestrictionLevel?)ar.RestrictionLevel, Category = (RestrictionLevel?)null };
        //
        // queryable = queryable.Concat(
        //     from ar in _dbContext.CategoryAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.CategoryId == c.Category.CategoryId
        //     select new { Forum = (RestrictionLevel?)null, Category = (RestrictionLevel?)ar.RestrictionLevel }
        // );
        //
        //
        // var queryable2 =
        //     from c in cte
        //     select new
        //     {
        //         AccessLevels = c,
        //         Restriction = userId == null ? null : queryable.FirstOrDefault()
        //     };
        //
        // var result = await queryable2.FirstAsyncLinqToDB(cancellationToken);
        //
        // var accessLevels = result.AccessLevels;
        // var restriction = result.Restriction;
        //
        // if (userId == null)
        // {
        //     if (accessLevels.Forum.AccessLevel > AccessLevel.Public)
        //         return new ForumAccessPolicyViolationError(accessLevels.Forum.ForumId, userId,
        //             accessLevels.Forum.AccessLevel);
        //     if (accessLevels.Category.AccessLevel > AccessLevel.Public)
        //         return new CategoryAccessPolicyViolationError(accessLevels.Category.CategoryId, userId,
        //             accessLevels.Forum.AccessLevel);
        // }
        // else if (restriction != null)
        // {
        //     if (restriction.Forum != null)
        //         return new ForumAccessRestrictedError(accessLevels.Forum.ForumId, userId.Value,
        //             restriction.Forum.Value);
        //     if (restriction.Category != null)
        //         return new CategoryAccessRestrictedError(accessLevels.Category.CategoryId, userId.Value,
        //             restriction.Category.Value);
        // }

        return Success.Instance;
    }

    public async Task<Result<Success, AccessPolicyViolationError, PolicyRestrictedError>> CheckUserAccessAsync(
        UserId? userId,
        PostId postId, CancellationToken cancellationToken)
    {
        // var cte = (
        //         from p in _dbContext.Posts.Where(e => e.PostId == postId)
        //         from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
        //         from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
        //         from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
        //         select new
        //         {
        //             Forum = new { f.ForumId, f.AccessLevel },
        //             Category = new { c.CategoryId, c.AccessLevel },
        //             Thread = new { t.ThreadId, t.AccessLevel }
        //         }
        //     )
        //     .AsCte();
        //
        // var queryable =
        //     from ar in _dbContext.ForumAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
        //     select new { ar.RestrictionLevel };
        //
        // queryable = queryable.Concat(
        //     from ar in _dbContext.CategoryAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.CategoryId == c.Category.CategoryId
        //     select new { ar.RestrictionLevel }
        // );
        //
        // queryable = queryable.Concat(
        //     from ar in _dbContext.ThreadAccessRestrictions
        //     from c in cte
        //     where ar.UserId == userId && ar.ThreadId == c.Thread.ThreadId
        //     select new { ar.RestrictionLevel }
        // );
        //
        // var queryable2 =
        //     from c in cte
        //     select new
        //     {
        //         AccessLevels = c,
        //         Restriction = userId == null ? null : queryable.FirstOrDefault()
        //     };
        //
        //
        // var result = await queryable2.FirstAsyncLinqToDB(cancellationToken);
        //
        // var accessLevels = result.AccessLevels;
        // var restriction = result.Restriction;
        //
        // if (userId == null)
        // {
        //     if (accessLevels.Forum.AccessLevel > AccessLevel.Public)
        //         return new ForumAccessPolicyViolationError(accessLevels.Forum.ForumId, userId,
        //             accessLevels.Forum.AccessLevel);
        //     if (accessLevels.Category.AccessLevel > AccessLevel.Public)
        //         return new CategoryAccessPolicyViolationError(accessLevels.Category.CategoryId, userId,
        //             accessLevels.Forum.AccessLevel);
        //     if (accessLevels.Thread.AccessLevel > AccessLevel.Public)
        //         return new ThreadAccessPolicyViolationError(accessLevels.Thread.ThreadId, userId,
        //             accessLevels.Forum.AccessLevel);
        // }
        // else
        // {
        //     if (restriction != null)
        //     {
        //         return new ThreadAccessRestrictedError(accessLevels.Thread.ThreadId, userId.Value,
        //             restriction.RestrictionLevel);
        //     }
        // }

        return Success.Instance;
    }

    public async Task<Result<Success, ForumNotFoundError, CategoryCreatePolicyViolationError>>
        CheckUserCanCreateCategoryAsync(UserId userId, ForumId forumId, CancellationToken cancellationToken)
    {
        // var queryable =
        //     from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
        //     from fmg in _dbContext.ForumModerationGrants
        //         .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
        //         .DefaultIfEmpty()
        //     select new
        //     {
        //         CanCreate = f.GetCategoryCreatePolicy() == CategoryCreatePolicy.Any || fmg.CreatedAt != null
        //     };
        //
        // var result = await queryable.FirstOrDefaultAsyncLinqToDB(cancellationToken);
        //
        // if (result == null) return new ForumNotFoundError(forumId);
        // if (!result.CanCreate) return new ForumModerationForbiddenError(userId, forumId);
        return Success.Instance;
    }

    public async Task<Result<Success, CategoryNotFoundError, ForumAccessPolicyViolationError, ForumPolicyRestrictedError
            ,
            CategoryAccessPolicyViolationError, CategoryPolicyRestrictedError, ThreadCreatePolicyViolationError>>
        CanUserCanCreateThreadAsync(UserId userId, CategoryId categoryId, CancellationToken cancellationToken)
    {
        // var queryable =
        //     from c in _dbContext.Categories.Where(e => e.CategoryId == categoryId)
        //     from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
        //     from fmg in _dbContext.ForumModerationGrants
        //         .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
        //         .DefaultIfEmpty()
        //     from cmg in _dbContext.CategoryModerationGrants
        //         .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
        //         .DefaultIfEmpty()
        //     from fag in _dbContext.ForumGrants
        //         .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
        //         .DefaultIfEmpty()
        //     from cag in _dbContext.CategoryGrants
        //         .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
        //         .DefaultIfEmpty()
        //     from far in _dbContext.ForumAccessRestrictions
        //         .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
        //         .DefaultIfEmpty()
        //     from car in _dbContext.CategoryAccessRestrictions
        //         .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
        //         .DefaultIfEmpty()
        //     select new
        //     {
        //         f.ForumId,
        //         ForumAccessLevel = f.AccessLevel,
        //         c.CategoryId,
        //         CategoryAccessLevel = c.AccessLevel,
        //         IsForumModerator = fmg.CreatedAt != null,
        //         IsCategoryModerator = cmg.CreatedAt != null,
        //         ForumAccessGrant = (DateTime?)fag.CreatedAt,
        //         CategoryAccessGrant = (DateTime?)cag.CreatedAt,
        //         ForumAccessRestriction = (RestrictionLevel?)far.RestrictionLevel,
        //         CategoryAccessRestriction = (RestrictionLevel?)car.RestrictionLevel,
        //         ThreadCreatePolicy = c.GetCreateThreadPolicy()
        //     };
        //
        // var result = await queryable.FirstOrDefaultAsyncLinqToDB(cancellationToken);
        //
        // if (result == null) return new CategoryNotFoundError(categoryId);
        //
        // if (!result.IsForumModerator)
        // {
        //     if (result.ForumAccessLevel == AccessLevel.Restricted && result.ForumAccessGrant == null)
        //         return new ForumAccessPolicyViolationError(result.ForumId, userId, result.ForumAccessLevel);
        //
        //     if (result.ForumAccessRestriction != null)
        //         return new ForumAccessRestrictedError(result.ForumId, userId, result.ForumAccessRestriction.Value);
        //
        //     if (!result.IsCategoryModerator)
        //     {
        //         if (result.ThreadCreatePolicy == ThreadCreatePolicy.Moderator)
        //             return new ThreadCreatePolicyViolationError(categoryId, result.ThreadCreatePolicy);
        //
        //         if (result.CategoryAccessLevel == AccessLevel.Restricted && result.CategoryAccessGrant == null)
        //             return new CategoryAccessPolicyViolationError(result.CategoryId, userId, result.CategoryAccessLevel);
        //
        //         if (result.CategoryAccessRestriction != null)
        //             return new CategoryAccessRestrictedError(result.CategoryId, userId,
        //                 result.CategoryAccessRestriction.Value);
        //     }
        // }
        //
        return Success.Instance;
    }

    public async Task<Result<Success, ThreadNotFoundError, AccessPolicyViolationError, PolicyRestrictedError,
            PostCreatePolicyViolationError>>
        CheckUserCanCreatePostAsync(UserId userId, ThreadId threadId, DateTime timestamp,
            CancellationToken cancellationToken)
    {
        var threadInfo = (
                from t in _dbContext.Threads.Where(e => e.ThreadId == threadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                select new
                {
                    f.ForumId,
                    c.CategoryId,
                    f.ForumPolicySetId,
                    c.CategoryPolicySetId,
                    t.ThreadPolicySetId,
                }
            )
            .AsCte();

        var result = (await (
                from ti in threadInfo
                from fps in _dbContext.ForumPolicySets.Where(e => e.ForumPolicySetId == ti.ForumPolicySetId)
                from cps in _dbContext.CategoryPolicySets
                    .Where(e => ti.CategoryPolicySetId.SqlIsNotNull() &&
                                e.CategoryPolicySetId == ti.CategoryPolicySetId)
                    .DefaultIfEmpty()
                from tps in _dbContext.ThreadPolicySets
                    .Where(e => ti.ThreadPolicySetId.SqlIsNotNull() && e.ThreadPolicySetId == ti.ThreadPolicySetId)
                    .DefaultIfEmpty()
                select new
                {
                    ti.ForumId,
                    ti.CategoryId,
                    ti.ForumPolicySetId,
                    ti.CategoryPolicySetId,
                    ti.ThreadPolicySetId,
                    ForumPolicies = new { fps.Access, fps.PostCreate },
                    CategoryPolicies = new { Access = (Policy?)cps.Access, PostCreate = (Policy?)cps.PostCreate },
                    ThreadPolicies = new { Access = (Policy?)tps.Access, PostCreate = (Policy?)tps.PostCreate },
                    forumGrants = (
                            from fg in _dbContext.ForumGrants
                            where fg.UserId == userId && fg.ForumId == ti.ForumId
                            group fg by new { fg.UserId, fg.ForumId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault(),
                    categoryGrants = (
                            from cg in _dbContext.CategoryGrants
                            where cg.UserId == userId && cg.CategoryId == ti.CategoryId
                            group cg by new { cg.UserId, cg.CategoryId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault(),
                    threadGrants = (
                            from tg in _dbContext.ThreadGrants
                            where tg.UserId == userId && tg.ThreadId == threadId
                            group tg by new { tg.UserId, tg.ThreadId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault(),
                    forumRestrictions = (
                            from fr in _dbContext.ForumRestrictions
                            where fr.UserId == userId && fr.ForumId == ti.ForumId &&
                                  (fr.ExpiredAt == null || fr.ExpiredAt > timestamp)
                            group fr by new { fr.UserId, fr.ForumId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault(),
                    categoryRestrictions = (
                            from cr in _dbContext.CategoryRestrictions
                            where cr.UserId == userId && cr.CategoryId == ti.CategoryId &&
                                  (cr.ExpiredAt == null || cr.ExpiredAt > timestamp)
                            group cr by new { cr.UserId, cr.CategoryId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault(),
                    threadRestrictions = (
                            from tr in _dbContext.ThreadRestrictions
                            where tr.UserId == userId && tr.ThreadId == threadId &&
                                  (tr.ExpiredAt == null || tr.ExpiredAt > timestamp)
                            group tr by new { tr.UserId, tr.ThreadId }
                            into g
                            select g.ArrayAggregate(e => (short)e.Policy, Sql.AggregateModifier.None).ToValue()
                        )
                        .FirstOrDefault()
                })
            .FirstOrDefaultAsyncLinqToDB(cancellationToken));

        if (result == null) return new ThreadNotFoundError(threadId);

        if (result.ThreadPolicies?.Access == Policy.Granted)
        {
            if (!result.threadGrants.Contains((byte)PolicyType.Access))
                return new ThreadAccessPolicyViolationError(threadId, userId, result.ThreadPolicies.Access.Value);
        }
        else if (result.CategoryPolicies?.Access == Policy.Granted)
        {
            if (!result.categoryGrants.Contains((byte)PolicyType.Access))
                return new CategoryAccessPolicyViolationError(result.CategoryId, userId,
                    result.CategoryPolicies.Access.Value);
        }
        else if (result.ForumPolicies.Access == Policy.Granted)
        {
            if (!result.forumGrants.Contains((byte)PolicyType.Access))
                return new ForumAccessPolicyViolationError(result.ForumId, userId, result.ForumPolicies.Access);
        }

        if (result.ThreadPolicies?.PostCreate == Policy.Granted)
        {
            if (!result.threadGrants.Contains((byte)PolicyType.PostCreate))
                return new PostCreatePolicyViolationError(threadId, userId, result.ThreadPolicies.PostCreate.Value);
        }
        else if (result.CategoryPolicies?.Access == Policy.Granted)
        {
            if (!result.categoryGrants.Contains((byte)PolicyType.PostCreate))
                return new PostCreatePolicyViolationError(threadId, userId, result.CategoryPolicies.Access.Value);
        }
        else if (result.ForumPolicies.Access == Policy.Granted)
        {
            if (!result.forumGrants.Contains((byte)PolicyType.PostCreate))
                return new PostCreatePolicyViolationError(threadId, userId, result.ForumPolicies.Access);
        }

        if (result.threadRestrictions != null && (result.threadRestrictions.Contains((byte)PolicyType.Access) ||
                                                  result.threadRestrictions.Contains((byte)PolicyType.PostCreate)))
            return new ThreadPolicyRestrictedError(threadId, userId, PolicyType.PostCreate);

        if (result.categoryRestrictions != null && (result.categoryRestrictions.Contains((byte)PolicyType.Access) ||
                                                    result.categoryRestrictions.Contains((byte)PolicyType.PostCreate)))
            return new CategoryPolicyRestrictedError(result.CategoryId, userId, PolicyType.PostCreate);

        if (result.forumRestrictions != null && (result.forumRestrictions.Contains((byte)PolicyType.Access) ||
                                                 result.forumRestrictions.Contains((byte)PolicyType.PostCreate)))
            return new ForumPolicyRestrictedError(result.ForumId, userId, PolicyType.PostCreate);

        return Success.Instance;
    }
}