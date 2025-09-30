using System.Linq.Expressions;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
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

    public async Task<Result<Success, ForumAccessLevelError, ForumAccessRestrictedError>>
        CheckUserAccessAsync(
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

        return Success.Instance;
    }

    public async
        Task<Result<Success, ForumAccessLevelError, CategoryAccessLevelError, ForumAccessRestrictedError,
            CategoryAccessRestrictedError>> CheckUserAccessAsync(UserId? userId, CategoryId categoryId,
            CancellationToken cancellationToken)
    {
        var cte = (
                from c in _dbContext.Categories.Where(e => e.CategoryId == categoryId)
                from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
                select new
                {
                    Forum = new { f.ForumId, f.AccessLevel },
                    Category = new { c.CategoryId, c.AccessLevel }
                }
            )
            .AsCte();

        var queryable =
            from ar in _dbContext.ForumAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.ForumId == c.Forum.ForumId
            select new { Forum = (RestrictionLevel?)ar.RestrictionLevel, Category = (RestrictionLevel?)null };

        queryable = queryable.Concat(
            from ar in _dbContext.CategoryAccessRestrictions
            from c in cte
            where ar.UserId == userId && ar.CategoryId == c.Category.CategoryId
            select new { Forum = (RestrictionLevel?)null, Category = (RestrictionLevel?)ar.RestrictionLevel }
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
        }
        else if (restriction != null)
        {
            if (restriction.Forum != null)
                return new ForumAccessRestrictedError(accessLevels.Forum.ForumId, userId.Value,
                    restriction.Forum.Value);
            if (restriction.Category != null)
                return new CategoryAccessRestrictedError(accessLevels.Category.CategoryId, userId.Value,
                    restriction.Category.Value);
        }

        return Success.Instance;
    }

    public async Task<Result<Success, AccessLevelError, AccessRestrictedError>> CheckUserAccessAsync(UserId? userId,
        PostId postId, CancellationToken cancellationToken)
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

        return Success.Instance;
    }

    public async Task<Result<Success, ForumNotFoundError, ForumModerationForbiddenError>>
        CheckUserCanCreateCategoryAsync(UserId userId, ForumId forumId, CancellationToken cancellationToken)
    {
        var queryable =
            from f in _dbContext.Forums.Where(e => e.ForumId == forumId)
            from fmg in _dbContext.ForumModerationGrants
                .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
                .DefaultIfEmpty()
            select new
            {
                CanCreate = f.GetCreateCategoryPolicy() == CategoryCreatePolicy.Any || fmg.CreatedAt != null
            };

        var result = await queryable.FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ForumNotFoundError(forumId);
        if (!result.CanCreate) return new ForumModerationForbiddenError(userId, forumId);
        return Success.Instance;
    }

    public Task<Result<Success, CategoryNotFoundError, AccessLevelError, AccessRestrictedError>>
        CheckUserWriteAccessAsync(UserId userId, CategoryId categoryId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Success, ThreadNotFoundError, AccessLevelError, AccessRestrictedError,
            PostCreatePolicyViolationError>>
        CheckUserCanCreatePostAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken)
    {
        var queryable =
            from t in _dbContext.Threads.Where(e => e.ThreadId == threadId)
            from c in _dbContext.Categories.Where(e => e.CategoryId == t.CategoryId)
            from f in _dbContext.Forums.Where(e => e.ForumId == c.ForumId)
            from fmg in _dbContext.ForumModerationGrants
                .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
                .DefaultIfEmpty()
            from cmg in _dbContext.CategoryModerationGrants
                .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
                .DefaultIfEmpty()
            from tmg in _dbContext.ThreadModerationGrants
                .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId)
                .DefaultIfEmpty()
            from fag in _dbContext.ForumAccessGrants
                .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
                .DefaultIfEmpty()
            from cag in _dbContext.CategoryAccessGrants
                .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
                .DefaultIfEmpty()
            from tag in _dbContext.ThreadAccessGrants
                .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId)
                .DefaultIfEmpty()
            from far in _dbContext.ForumAccessRestrictions
                .Where(e => e.UserId == userId && e.ForumId == f.ForumId)
                .DefaultIfEmpty()
            from car in _dbContext.CategoryAccessRestrictions
                .Where(e => e.UserId == userId && e.CategoryId == c.CategoryId)
                .DefaultIfEmpty()
            from tar in _dbContext.ThreadAccessRestrictions
                .Where(e => e.UserId == userId && e.ThreadId == t.ThreadId)
                .DefaultIfEmpty()
            select new
            {
                f.ForumId,
                ForumAccessLevel = f.AccessLevel,
                c.CategoryId,
                CategoryAccessLevel = c.AccessLevel,
                IsForumModerator = fmg.CreatedAt != null,
                IsCategoryModerator = cmg.CreatedAt != null,
                IsThreadModerator = tmg.CreatedAt != null,
                ForumAccessGrant = (DateTime?)fag.CreatedAt,
                CategoryAccessGrant = (DateTime?)cag.CreatedAt,
                ThreadAccessGrant = (DateTime?)tag.CreatedAt,
                ForumAccessRestriction = (RestrictionLevel?)far.RestrictionLevel,
                CategoryAccessRestriction = (RestrictionLevel?)car.RestrictionLevel,
                ThreadAccessRestriction = (RestrictionLevel?)tar.RestrictionLevel,
                PostCreatePolicy = t.GetCreatePostPolicy()
            };

        var result = await queryable.FirstOrDefaultAsyncLinqToDB(cancellationToken);

        if (result == null) return new ThreadNotFoundError(threadId);

        if (!result.IsForumModerator)
        {
            if (result.ForumAccessLevel == AccessLevel.Restricted && result.ForumAccessGrant == null)
                return new ForumAccessLevelError(result.ForumId, userId, result.ForumAccessLevel);
            if (result.ForumAccessRestriction != null)
                return new ForumAccessRestrictedError(result.ForumId, userId, result.ForumAccessRestriction.Value);
            if (!result.IsCategoryModerator)
            {
                if (result.CategoryAccessLevel == AccessLevel.Restricted && result.CategoryAccessGrant == null)
                    return new CategoryAccessLevelError(result.CategoryId, userId, result.CategoryAccessLevel);
                if (result.CategoryAccessRestriction != null)
                    return new CategoryAccessRestrictedError(result.CategoryId, userId,
                        result.CategoryAccessRestriction.Value);
                if (!result.IsThreadModerator)
                {
                    if (result.PostCreatePolicy == PostCreatePolicy.Moderator)
                        return new PostCreatePolicyViolationError(threadId, result.PostCreatePolicy);
                    if (result.ThreadAccessRestriction != null)
                        return new ThreadAccessRestrictedError(threadId, userId, result.ThreadAccessRestriction.Value);
                }
            }
        }

        return Success.Instance;
    }
}

file static class QueryableExtensions
{
    [ExpressionMethod(nameof(GetCreateCategoryPolicyImpl))]
    public static CategoryCreatePolicy GetCreateCategoryPolicy(this Forum forum)
    {
        throw new LinqToDBException($"{nameof(GetCreateCategoryPolicy)} server side only");
    }

    private static Expression<Func<Forum, CategoryCreatePolicy>> GetCreateCategoryPolicyImpl()
        => forum => Sql.Property<CategoryCreatePolicy>(forum, "policies_category_create");

    [ExpressionMethod(nameof(GetCreatePostPolicyImpl))]
    public static PostCreatePolicy GetCreatePostPolicy(this Thread thread)
    {
        throw new LinqToDBException($"{nameof(GetCreatePostPolicy)} server side only");
    }

    private static Expression<Func<Thread, PostCreatePolicy>> GetCreatePostPolicyImpl()
        => thread => Sql.Property<PostCreatePolicy>(thread, "policies_post_create");
}