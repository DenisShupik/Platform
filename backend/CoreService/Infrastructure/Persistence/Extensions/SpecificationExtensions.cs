using System.Linq.Expressions;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using LinqToDB;
using Shared.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Infrastructure.Persistence.Extensions;

public static class SpecificationExtensions
{
    public static IQueryable<Forum> WhereCanRead(
        this IQueryable<Forum> forums,
        ApplicationDbContext dbContext,
        ActorContext? actor,
        DateTime evaluatedAt)
    {
        if (actor is null) return forums;
        var actorId = actor.Value.UserId;
        return forums.Where(forum =>
            !dbContext.ForumSanctions.Any(sanction =>
                sanction.UserId == actorId &&
                sanction.Type == ForumSanctionType.NoAccess &&
                sanction.RevokedAt == null &&
                sanction.IssuedAt <= evaluatedAt &&
                (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt) &&
                (sanction.ScopeType == AuthorizationScopeType.Platform ||
                 sanction.ScopeType == AuthorizationScopeType.Forum && sanction.ForumId == forum.ForumId)));
    }

    public static IQueryable<Category> WhereCanRead(
        this IQueryable<Category> categories,
        ApplicationDbContext dbContext,
        ActorContext? actor,
        DateTime evaluatedAt)
    {
        if (actor is null) return categories;
        var actorId = actor.Value.UserId;
        return categories.Where(category =>
            !dbContext.ForumSanctions.Any(sanction =>
                sanction.UserId == actorId &&
                sanction.Type == ForumSanctionType.NoAccess &&
                sanction.RevokedAt == null &&
                sanction.IssuedAt <= evaluatedAt &&
                (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt) &&
                (sanction.ScopeType == AuthorizationScopeType.Platform ||
                 sanction.ScopeType == AuthorizationScopeType.Forum && sanction.ForumId == category.ForumId ||
                 sanction.ScopeType == AuthorizationScopeType.Category &&
                 sanction.CategoryId == category.CategoryId)));
    }

    [ExpressionMethod(nameof(CanReadThreadImpl))]
    public static bool CanReadThread(
        this ApplicationDbContext dbContext,
        Thread thread,
        ActorContext? actor,
        DateTime evaluatedAt) =>
        throw new InvalidOperationException("This method should only be translated to SQL");

    private static Expression<Func<ApplicationDbContext, Thread, ActorContext?, DateTime, bool>>
        CanReadThreadImpl() =>
        (dbContext, thread, actor, evaluatedAt) =>
            actor == null
                ? thread.State == ThreadState.Approved
                : !dbContext.ForumSanctions.Any(sanction =>
                      sanction.UserId == actor.Value.UserId &&
                      sanction.Type == ForumSanctionType.NoAccess &&
                      sanction.RevokedAt == null &&
                      sanction.IssuedAt <= evaluatedAt &&
                      (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt) &&
                      (sanction.ScopeType == AuthorizationScopeType.Platform ||
                       sanction.ScopeType == AuthorizationScopeType.Category &&
                       sanction.CategoryId == thread.CategoryId ||
                       sanction.ScopeType == AuthorizationScopeType.Thread &&
                       sanction.ThreadId == thread.ThreadId ||
                       sanction.ScopeType == AuthorizationScopeType.Forum &&
                       dbContext.Categories.Any(category =>
                           category.CategoryId == thread.CategoryId &&
                           category.ForumId == sanction.ForumId))) &&
                  (thread.State == ThreadState.Approved ||
                   thread.CreatedBy == actor.Value.UserId ||
                   dbContext.CapabilityGrants.Any(grant =>
                       grant.UserId == actor.Value.UserId &&
                       grant.Capability == CapabilityCode.ViewUnpublishedThreads &&
                       grant.RevokedAt == null &&
                       grant.GrantedAt <= evaluatedAt &&
                       (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt) &&
                       (grant.ScopeType == AuthorizationScopeType.Platform ||
                        grant.ScopeType == AuthorizationScopeType.Category &&
                        grant.CategoryId == thread.CategoryId ||
                        grant.ScopeType == AuthorizationScopeType.Thread &&
                        grant.ThreadId == thread.ThreadId ||
                        grant.ScopeType == AuthorizationScopeType.Forum &&
                        dbContext.Categories.Any(category =>
                            category.CategoryId == thread.CategoryId &&
                            category.ForumId == grant.ForumId))));
}
