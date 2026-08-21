using CoreService.Application.Interfaces;
using CoreService.Application.Dtos;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class CapabilityGrantRepository(WriteApplicationDbContext dbContext) : ICapabilityGrantRepository
{
    public async Task<IReadOnlySet<(CapabilityCode Capability, AuthorizationScopeType ScopeType)>>
        GetActiveCapabilityScopesAsync(
            UserId userId,
            IReadOnlySet<CapabilityCode> capabilities,
            DateTime evaluatedAt,
            CancellationToken cancellationToken)
    {
        var entries = await dbContext.CapabilityGrants
            .Where(grant =>
                grant.UserId == userId &&
                capabilities.Contains(grant.Capability) &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt))
            .Select(grant => new { grant.Capability, grant.ScopeType })
            .Distinct()
            .ToListAsyncEF(cancellationToken);

        return entries.Select(entry => (entry.Capability, entry.ScopeType)).ToHashSet();
    }

    public Task<bool> HasActiveCapabilityAsync(
        UserId userId,
        CapabilityCode capability,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return dbContext.CapabilityGrants.AnyAsyncEF(
            grant =>
                grant.UserId == userId &&
                grant.Capability == capability &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt) &&
                (grant.ScopeType == AuthorizationScopeType.Platform ||
                 scope.ForumId != null &&
                 grant.ScopeType == AuthorizationScopeType.Forum && grant.ForumId == scope.ForumId ||
                 scope.CategoryId != null &&
                 grant.ScopeType == AuthorizationScopeType.Category && grant.CategoryId == scope.CategoryId ||
                 scope.ThreadId != null &&
                 grant.ScopeType == AuthorizationScopeType.Thread && grant.ThreadId == scope.ThreadId),
            cancellationToken);
    }

    public async Task<IReadOnlySet<CapabilityCode>> GetActiveCapabilitiesAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        var capabilities = await dbContext.CapabilityGrants
            .Where(grant =>
                grant.UserId == userId &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt) &&
                (grant.ScopeType == AuthorizationScopeType.Platform ||
                 scope.ForumId != null &&
                 grant.ScopeType == AuthorizationScopeType.Forum && grant.ForumId == scope.ForumId ||
                 scope.CategoryId != null &&
                 grant.ScopeType == AuthorizationScopeType.Category && grant.CategoryId == scope.CategoryId ||
                 scope.ThreadId != null &&
                 grant.ScopeType == AuthorizationScopeType.Thread && grant.ThreadId == scope.ThreadId))
            .Select(grant => grant.Capability)
            .Distinct()
            .ToListAsyncEF(cancellationToken);

        return capabilities.ToHashSet();
    }

    public async Task<IReadOnlyList<CategoryModeratorAppointmentDto>> GetActiveCategoryModeratorAppointmentsAsync(
        CategoryId categoryId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.SourceType == GrantSourceType.CategoryModeratorAppointment &&
                grant.CategoryId == categoryId &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt))
            .Select(grant => new CategoryModeratorAppointmentDto
            {
                AssignmentId = grant.AssignmentId,
                UserId = grant.UserId,
                GrantedBy = grant.GrantedBy!.Value,
                GrantedAt = grant.GrantedAt,
                ValidUntil = grant.ValidUntil
            })
            .Distinct()
            .OrderBy(appointment => appointment.GrantedAt)
            .ThenBy(appointment => appointment.AssignmentId)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<ForumModeratorAppointmentDto>> GetActiveForumModeratorAppointmentsAsync(
        ForumId forumId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.SourceType == GrantSourceType.ForumModeratorAppointment &&
                grant.ForumId == forumId &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt))
            .Select(grant => new ForumModeratorAppointmentDto
            {
                AssignmentId = grant.AssignmentId,
                UserId = grant.UserId,
                GrantedBy = grant.GrantedBy!.Value,
                GrantedAt = grant.GrantedAt,
                ValidUntil = grant.ValidUntil
            })
            .Distinct()
            .OrderBy(appointment => appointment.GrantedAt)
            .ThenBy(appointment => appointment.AssignmentId)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformAdministratorAppointmentDto>>
        GetActivePlatformAdministratorAppointmentsAsync(
            DateTime evaluatedAt,
            CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                (grant.SourceType == GrantSourceType.PlatformAdministratorAppointment ||
                 grant.SourceType == GrantSourceType.PlatformAdministratorBootstrap) &&
                grant.ScopeType == AuthorizationScopeType.Platform &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt))
            .Select(grant => new PlatformAdministratorAppointmentDto
            {
                AssignmentId = grant.AssignmentId,
                UserId = grant.UserId,
                GrantedBy = grant.GrantedBy,
                GrantedAt = grant.GrantedAt,
                WasBootstrapped = grant.SourceType == GrantSourceType.PlatformAdministratorBootstrap
            })
            .Distinct()
            .OrderBy(appointment => appointment.GrantedAt)
            .ThenBy(appointment => appointment.AssignmentId)
            .ToListAsyncEF(cancellationToken);
    }

    public Task<bool> HasAnyActivePlatformAdministratorAsync(
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return dbContext.CapabilityGrants.AnyAsyncEF(
            grant =>
                (grant.SourceType == GrantSourceType.PlatformAdministratorAppointment ||
                 grant.SourceType == GrantSourceType.PlatformAdministratorBootstrap) &&
                grant.ScopeType == AuthorizationScopeType.Platform &&
                grant.Capability == CapabilityCode.ManageAuthorization &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt),
            cancellationToken);
    }

    public async Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedPlatformAdministratorGrantsAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.UserId == userId &&
                (grant.SourceType == GrantSourceType.PlatformAdministratorAppointment ||
                 grant.SourceType == GrantSourceType.PlatformAdministratorBootstrap) &&
                grant.ScopeType == AuthorizationScopeType.Platform &&
                grant.RevokedAt == null)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedCategoryModeratorGrantsAsync(
        UserId userId,
        CategoryId categoryId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.UserId == userId &&
                grant.SourceType == GrantSourceType.CategoryModeratorAppointment &&
                grant.CategoryId == categoryId &&
                grant.RevokedAt == null)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedForumModeratorGrantsAsync(
        UserId userId,
        ForumId forumId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.UserId == userId &&
                grant.SourceType == GrantSourceType.ForumModeratorAppointment &&
                grant.ForumId == forumId &&
                grant.RevokedAt == null)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<CapabilityGrantDto>> GetActiveDirectGrantsAsync(
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken)
    {
        return await dbContext.CapabilityGrants
            .Where(grant =>
                grant.SourceType == GrantSourceType.Direct &&
                grant.ScopeType == scope.Type &&
                grant.ForumId == scope.ForumId &&
                grant.CategoryId == scope.CategoryId &&
                grant.ThreadId == scope.ThreadId &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt))
            .Select(grant => new CapabilityGrantDto
            {
                CapabilityGrantId = grant.CapabilityGrantId,
                AssignmentId = grant.AssignmentId,
                UserId = grant.UserId,
                Capability = grant.Capability,
                ScopeType = grant.ScopeType,
                ForumId = grant.ForumId,
                CategoryId = grant.CategoryId,
                ThreadId = grant.ThreadId,
                SourceType = grant.SourceType,
                GrantedBy = grant.GrantedBy!.Value,
                GrantedAt = grant.GrantedAt,
                ValidUntil = grant.ValidUntil,
                RevokedBy = grant.RevokedBy,
                RevokedAt = grant.RevokedAt
            })
            .OrderBy(grant => grant.GrantedAt)
            .ThenBy(grant => grant.CapabilityGrantId)
            .ToListAsyncEF(cancellationToken);
    }

    public async Task<IReadOnlyList<CapabilityGrantDto>> GetDirectGrantHistoryAsync(
        AuthorizationScope scope,
        CancellationToken cancellationToken) =>
        await ProjectGrants(dbContext.CapabilityGrants.Where(grant =>
                grant.SourceType == GrantSourceType.Direct &&
                grant.ScopeType == scope.Type &&
                grant.ForumId == scope.ForumId &&
                grant.CategoryId == scope.CategoryId &&
                grant.ThreadId == scope.ThreadId))
            .OrderByDescending(grant => grant.GrantedAt)
            .ThenByDescending(grant => grant.CapabilityGrantId)
            .ToListAsyncEF(cancellationToken);

    public async Task<IReadOnlyList<CapabilityGrantDto>> GetEffectiveGrantsAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken) =>
        await ProjectGrants(dbContext.CapabilityGrants.Where(grant =>
                grant.UserId == userId &&
                grant.RevokedAt == null &&
                grant.GrantedAt <= evaluatedAt &&
                (grant.ValidUntil == null || grant.ValidUntil > evaluatedAt) &&
                (grant.ScopeType == AuthorizationScopeType.Platform ||
                 scope.ForumId != null && grant.ScopeType == AuthorizationScopeType.Forum &&
                 grant.ForumId == scope.ForumId ||
                 scope.CategoryId != null && grant.ScopeType == AuthorizationScopeType.Category &&
                 grant.CategoryId == scope.CategoryId ||
                 scope.ThreadId != null && grant.ScopeType == AuthorizationScopeType.Thread &&
                 grant.ThreadId == scope.ThreadId)))
            .OrderBy(grant => grant.Capability)
            .ThenBy(grant => grant.ScopeType)
            .ThenBy(grant => grant.GrantedAt)
            .ToListAsyncEF(cancellationToken);

    private static IQueryable<CapabilityGrantDto> ProjectGrants(IQueryable<CapabilityGrant> query) =>
        query.Select(grant => new CapabilityGrantDto
        {
            CapabilityGrantId = grant.CapabilityGrantId,
            AssignmentId = grant.AssignmentId,
            UserId = grant.UserId,
            Capability = grant.Capability,
            ScopeType = grant.ScopeType,
            ForumId = grant.ForumId,
            CategoryId = grant.CategoryId,
            ThreadId = grant.ThreadId,
            SourceType = grant.SourceType,
            GrantedBy = grant.GrantedBy,
            GrantedAt = grant.GrantedAt,
            ValidUntil = grant.ValidUntil,
            RevokedBy = grant.RevokedBy,
            RevokedAt = grant.RevokedAt
        });

    public Task<CapabilityGrant?> GetUnrevokedDirectGrantAsync(
        UserId userId,
        CapabilityCode capability,
        AuthorizationScope scope,
        CancellationToken cancellationToken)
    {
        return dbContext.CapabilityGrants.FirstOrDefaultAsyncEF(
            grant =>
                grant.UserId == userId &&
                grant.Capability == capability &&
                grant.SourceType == GrantSourceType.Direct &&
                grant.ScopeType == scope.Type &&
                grant.ForumId == scope.ForumId &&
                grant.CategoryId == scope.CategoryId &&
                grant.ThreadId == scope.ThreadId &&
                grant.RevokedAt == null,
            cancellationToken);
    }

    public Task<CapabilityGrant?> GetUnrevokedDirectGrantAsync(
        CapabilityGrantId capabilityGrantId,
        CancellationToken cancellationToken)
    {
        return dbContext.CapabilityGrants.FirstOrDefaultAsyncEF(
            grant =>
                grant.CapabilityGrantId == capabilityGrantId &&
                grant.SourceType == GrantSourceType.Direct &&
                grant.RevokedAt == null,
            cancellationToken);
    }

    public void AddRange(IEnumerable<CapabilityGrant> grants) => dbContext.CapabilityGrants.AddRange(grants);
}
