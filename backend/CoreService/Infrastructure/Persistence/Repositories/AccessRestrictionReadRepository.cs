using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using UserService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using Shared.Domain.Helpers;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class AccessRestrictionReadRepository : IAccessRestrictionReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public AccessRestrictionReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<Success, AccessRestrictedError>> CanUserPostInThreadAsync(UserId userId,
        ThreadId threadId, CancellationToken cancellationToken)
    {
        var categoriesCte = (
                from t in _dbContext.Threads.Where(e => e.ThreadId == threadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                select new { c.CategoryId, c.ForumId }
            )
            .AsCte();

        var queryable =
            from ar in _dbContext.ForumAccessRestrictions
            from c in categoriesCte
            where ar.UserId == userId && ar.ForumId == c.ForumId
            select new { ar.RestrictionLevel };

        queryable = queryable.Concat(
            from ar in _dbContext.CategoryAccessRestrictions
            from c in categoriesCte
            where ar.UserId == userId && ar.CategoryId == c.CategoryId
            select new { ar.RestrictionLevel }
        );

        queryable = queryable.Concat(
            from ar in _dbContext.ThreadAccessRestrictions
            where ar.UserId == userId && ar.ThreadId == threadId
            select new { ar.RestrictionLevel }
        );

        var accessRestriction = await queryable.FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (accessRestriction != null)
            return new ThreadAccessRestrictedError(threadId, userId, accessRestriction.RestrictionLevel);

        return OneOfHelper.Success;
    }

    public async Task<OneOf<Success, ForumAccessLevelError, ForumAccessRestrictedError>> CheckUserAccessAsync(
        UserId? userId, ForumId forumId, CancellationToken cancellationToken)
    {
        var cte = (
                from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
                select new
                {
                    Forum = new { f.ForumId, f.AccessLevel }
                }
            )
            .AsCte();

        var queryable =
            from ar in _dbContext.ForumAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
            select new { ar.RestrictionLevel };

        var queryable2 =
            from c in cte
            select new
            {
                AccessLevels = c,
                Restriction = userId == null ? null : queryable.FirstOrDefault()
            };


        var result = await queryable2.FirstAsyncLinqToDB(cancellationToken);

        var accessLevels = result.AccessLevels;
        var restriction = result.Restriction;

        if (userId == null)
        {
            if (accessLevels.Forum.AccessLevel > AccessLevel.Public)
                return new ForumAccessLevelError(accessLevels.Forum.ForumId, userId,
                    accessLevels.Forum.AccessLevel);
        }
        else
        {
            if (restriction != null)
            {
                return new ForumAccessRestrictedError(accessLevels.Forum.ForumId, userId.Value,
                    restriction.RestrictionLevel);
            }
        }

        return OneOfHelper.Success;
    }

    public async Task<OneOf<Success, AccessLevelError, AccessRestrictedError>> CheckUserAccessAsync(UserId? userId,
        PostId postId,
        CancellationToken cancellationToken)
    {
        var cte = (
                from p in _dbContext.Posts.Where(e => e.PostId == postId)
                from t in _dbContext.Threads.Where(e => e.ThreadId == p.ThreadId)
                from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                select new
                {
                    Forum = new { f.ForumId, f.AccessLevel },
                    Category = new { c.CategoryId, c.AccessLevel },
                    Thread = new { t.ThreadId, t.AccessLevel }
                }
            )
            .AsCte();

        var queryable =
            from ar in _dbContext.ForumAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
            select new { ar.RestrictionLevel };

        queryable = queryable.Concat(
            from ar in _dbContext.CategoryAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.CategoryId == c.Category.CategoryId
            select new { ar.RestrictionLevel }
        );

        queryable = queryable.Concat(
            from ar in _dbContext.ThreadAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.ThreadId == c.Thread.ThreadId
            select new { ar.RestrictionLevel }
        );

        var queryable2 =
            from c in cte
            select new
            {
                AccessLevels = c,
                Restriction = userId == null ? null : queryable.FirstOrDefault()
            };


        var result = await queryable2.FirstAsyncLinqToDB(cancellationToken);

        var accessLevels = result.AccessLevels;
        var restriction = result.Restriction;

        if (userId == null)
        {
            if (accessLevels.Forum.AccessLevel > AccessLevel.Public)
                return new ForumAccessLevelError(accessLevels.Forum.ForumId, userId,
                    accessLevels.Forum.AccessLevel);
            if (accessLevels.Category.AccessLevel > AccessLevel.Public)
                return new CategoryAccessLevelError(accessLevels.Category.CategoryId, userId,
                    accessLevels.Forum.AccessLevel);
            if (accessLevels.Thread.AccessLevel > AccessLevel.Public)
                return new ThreadAccessLevelError(accessLevels.Thread.ThreadId, userId,
                    accessLevels.Forum.AccessLevel);
        }
        else
        {
            if (restriction != null)
            {
                return new ThreadAccessRestrictedError(accessLevels.Thread.ThreadId, userId.Value,
                    restriction.RestrictionLevel);
            }
        }

        return OneOfHelper.Success;
    }
}