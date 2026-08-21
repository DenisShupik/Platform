using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumSanctionRepository(WriteApplicationDbContext dbContext) : IForumSanctionRepository
{
    public Task<bool> IsParticipationRestrictedAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken) =>
        ActiveFor(userId, scope, evaluatedAt).AnyAsyncEF(cancellationToken);

    public Task<bool> IsThreadParticipationRestrictedAsync(
        UserId userId,
        ThreadId threadId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return (
                from thread in dbContext.Threads
                from category in dbContext.Categories.Where(category => category.CategoryId == thread.CategoryId)
                where thread.ThreadId == threadId
                select dbContext.ForumSanctions.Any(sanction =>
                    sanction.UserId == userId &&
                    sanction.RevokedAt == null &&
                    sanction.IssuedAt <= evaluatedAt &&
                    (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt) &&
                    (sanction.ScopeType == AuthorizationScopeType.Platform ||
                     sanction.ScopeType == AuthorizationScopeType.Forum && sanction.ForumId == category.ForumId ||
                     sanction.ScopeType == AuthorizationScopeType.Category && sanction.CategoryId == thread.CategoryId ||
                     sanction.ScopeType == AuthorizationScopeType.Thread && sanction.ThreadId == thread.ThreadId)))
            .FirstOrDefaultAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<ForumSanctionDto>> GetActiveAsync(
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return await dbContext.ForumSanctions
            .Where(sanction =>
                sanction.ScopeType == scope.Type &&
                sanction.ForumId == scope.ForumId &&
                sanction.CategoryId == scope.CategoryId &&
                sanction.ThreadId == scope.ThreadId &&
                sanction.RevokedAt == null &&
                sanction.IssuedAt <= evaluatedAt &&
                (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt))
            .Select(sanction => new ForumSanctionDto
            {
                ForumSanctionId = sanction.ForumSanctionId,
                UserId = sanction.UserId,
                Type = sanction.Type,
                ScopeType = sanction.ScopeType,
                ForumId = sanction.ForumId,
                CategoryId = sanction.CategoryId,
                ThreadId = sanction.ThreadId,
                Reason = sanction.Reason,
                IssuedBy = sanction.IssuedBy,
                IssuedAt = sanction.IssuedAt,
                ValidUntil = sanction.ValidUntil,
                RevokedBy = sanction.RevokedBy,
                RevokedAt = sanction.RevokedAt
            })
            .OrderBy(sanction => sanction.IssuedAt)
            .ThenBy(sanction => sanction.ForumSanctionId)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<ForumSanctionDto>> GetHistoryAsync(
        AuthorizationScope scope,
        CancellationToken cancellationToken)
    {
        return await dbContext.ForumSanctions
            .Where(sanction =>
                sanction.ScopeType == scope.Type &&
                sanction.ForumId == scope.ForumId &&
                sanction.CategoryId == scope.CategoryId &&
                sanction.ThreadId == scope.ThreadId)
            .Select(sanction => new ForumSanctionDto
            {
                ForumSanctionId = sanction.ForumSanctionId,
                UserId = sanction.UserId,
                Type = sanction.Type,
                ScopeType = sanction.ScopeType,
                ForumId = sanction.ForumId,
                CategoryId = sanction.CategoryId,
                ThreadId = sanction.ThreadId,
                Reason = sanction.Reason,
                IssuedBy = sanction.IssuedBy,
                IssuedAt = sanction.IssuedAt,
                ValidUntil = sanction.ValidUntil,
                RevokedBy = sanction.RevokedBy,
                RevokedAt = sanction.RevokedAt
            })
            .OrderByDescending(sanction => sanction.IssuedAt)
            .ThenByDescending(sanction => sanction.ForumSanctionId)
            .ToListAsyncEF(cancellationToken);
    }

    public Task<ForumSanction?> GetUnrevokedAsync(
        UserId userId,
        ForumSanctionType type,
        AuthorizationScope scope,
        CancellationToken cancellationToken)
    {
        return dbContext.ForumSanctions.FirstOrDefaultAsyncEF(
            sanction =>
                sanction.UserId == userId &&
                sanction.Type == type &&
                sanction.ScopeType == scope.Type &&
                sanction.ForumId == scope.ForumId &&
                sanction.CategoryId == scope.CategoryId &&
                sanction.ThreadId == scope.ThreadId &&
                sanction.RevokedAt == null,
            cancellationToken);
    }

    public Task<ForumSanction?> GetUnrevokedAsync(
        ForumSanctionId forumSanctionId,
        CancellationToken cancellationToken)
    {
        return dbContext.ForumSanctions.FirstOrDefaultAsyncEF(
            sanction => sanction.ForumSanctionId == forumSanctionId && sanction.RevokedAt == null,
            cancellationToken);
    }

    public void Add(ForumSanction sanction) => dbContext.ForumSanctions.Add(sanction);

    private IQueryable<ForumSanction> ActiveFor(UserId userId, AuthorizationScope scope, DateTime evaluatedAt) =>
        dbContext.ForumSanctions.Where(sanction =>
            sanction.UserId == userId &&
            sanction.RevokedAt == null &&
            sanction.IssuedAt <= evaluatedAt &&
            (sanction.ValidUntil == null || sanction.ValidUntil > evaluatedAt) &&
            (sanction.ScopeType == AuthorizationScopeType.Platform ||
             scope.ForumId != null && sanction.ScopeType == AuthorizationScopeType.Forum &&
             sanction.ForumId == scope.ForumId ||
             scope.CategoryId != null && sanction.ScopeType == AuthorizationScopeType.Category &&
             sanction.CategoryId == scope.CategoryId ||
             scope.ThreadId != null && sanction.ScopeType == AuthorizationScopeType.Thread &&
             sanction.ThreadId == scope.ThreadId));
}
