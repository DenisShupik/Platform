using CoreService.Domain.Entities;
using CoreService.Application.Dtos;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

/// <summary>
/// Порт хранилища доменных назначений. Реализация и способ хранения остаются за Infrastructure.
/// </summary>
public interface ICapabilityGrantRepository
{
    Task<IReadOnlySet<(CapabilityCode Capability, AuthorizationScopeType ScopeType)>>
        GetActiveCapabilityScopesAsync(
            UserId userId,
            IReadOnlySet<CapabilityCode> capabilities,
            DateTime evaluatedAt,
            CancellationToken cancellationToken);

    Task<bool> HasActiveCapabilityAsync(
        UserId userId,
        CapabilityCode capability,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<CapabilityCode>> GetActiveCapabilitiesAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryModeratorAppointmentDto>> GetActiveCategoryModeratorAppointmentsAsync(
        CategoryId categoryId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ForumModeratorAppointmentDto>> GetActiveForumModeratorAppointmentsAsync(
        ForumId forumId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PlatformAdministratorAppointmentDto>> GetActivePlatformAdministratorAppointmentsAsync(
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<bool> HasAnyActivePlatformAdministratorAsync(
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedPlatformAdministratorGrantsAsync(
        UserId userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedCategoryModeratorGrantsAsync(
        UserId userId,
        CategoryId categoryId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrant>> GetUnrevokedForumModeratorGrantsAsync(
        UserId userId,
        ForumId forumId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrantDto>> GetActiveDirectGrantsAsync(
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrantDto>> GetDirectGrantHistoryAsync(
        AuthorizationScope scope,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CapabilityGrantDto>> GetEffectiveGrantsAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<CapabilityGrant?> GetUnrevokedDirectGrantAsync(
        UserId userId,
        CapabilityCode capability,
        AuthorizationScope scope,
        CancellationToken cancellationToken);

    Task<CapabilityGrant?> GetUnrevokedDirectGrantAsync(
        CapabilityGrantId capabilityGrantId,
        CancellationToken cancellationToken);

    void AddRange(IEnumerable<CapabilityGrant> grants);
}
