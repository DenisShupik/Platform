using CoreService.Application.Dtos;
using CoreService.Domain.Entities;
using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IForumSanctionRepository
{
    Task<bool> IsParticipationRestrictedAsync(
        UserId userId,
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<bool> IsThreadParticipationRestrictedAsync(
        UserId userId,
        ThreadId threadId,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ForumSanctionDto>> GetActiveAsync(
        AuthorizationScope scope,
        DateTime evaluatedAt,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ForumSanctionDto>> GetHistoryAsync(
        AuthorizationScope scope,
        CancellationToken cancellationToken);

    Task<ForumSanction?> GetUnrevokedAsync(
        UserId userId,
        ForumSanctionType type,
        AuthorizationScope scope,
        CancellationToken cancellationToken);

    Task<ForumSanction?> GetUnrevokedAsync(
        ForumSanctionId forumSanctionId,
        CancellationToken cancellationToken);

    void Add(ForumSanction sanction);
}
