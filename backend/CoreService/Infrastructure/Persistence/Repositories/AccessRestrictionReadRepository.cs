using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
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

    public async Task<OneOf<Success, ThreadAccessRestrictedError>> CanUserPostInThreadAsync(UserId userId,
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
            select 1;

        queryable = queryable.Concat(
                from ar in _dbContext.CategoryAccessRestrictions
                from c in categoriesCte
                where ar.UserId == userId && ar.CategoryId == c.CategoryId
                select 1
                );

        queryable = queryable.Concat(
            from ar in _dbContext.ThreadAccessRestrictions
            where ar.UserId == userId && ar.ThreadId == threadId
            select 1
        );
        
        // var hasAccess = await _dbContext.Threads
        //     .Where(t => t.ThreadId == threadId)
        //     .InnerJoin(_dbContext.GetTable<Category>(),
        //         (t, c) => t.CategoryId == c.CategoryId,
        //         (t, c) => new { t, c })
        //     .Select(x => _dbContext.AccessRestrictions
        //         .Where(ar => ar.UserId == userId)
        //         .Any(ar =>
        //             (ar is ThreadAccessRestriction && ((ThreadAccessRestriction)ar).ThreadId == threadId) ||
        //             (ar is CategoryAccessRestriction && ((CategoryAccessRestriction)ar).CategoryId == x.c.CategoryId) ||
        //             ((ForumAccessRestriction)ar).ForumId == x.c.ForumId)
        //     )
        //     .AnyAsyncLinqToDB(token: cancellationToken);

        if (await queryable.AnyAsyncLinqToDB(cancellationToken))
            return new ThreadAccessRestrictedError(threadId, userId);

        return OneOfHelper.Success;
    }
}