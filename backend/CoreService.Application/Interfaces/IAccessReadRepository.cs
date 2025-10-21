using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessReadRepository
{
    Task<Dictionary<PolicyType, bool>> GetPortalPermissionsAsync(GetPortalPermissionsQuery query,
        CancellationToken cancellationToken);

    Task<Result<Dictionary<PolicyType, bool>, ForumNotFoundError>> GetForumPermissionsAsync(
        GetForumPermissionsQuery query, CancellationToken cancellationToken);

    Task<Result<Dictionary<PolicyType, bool>, CategoryNotFoundError>> GetCategoryPermissionsAsync(
        GetCategoryPermissionsQuery query, CancellationToken cancellationToken);

    Task<Result<Dictionary<PolicyType, bool>, ThreadNotFoundError>> GetThreadPermissionsAsync(
        GetThreadPermissionsQuery query, CancellationToken cancellationToken);

    Task<Result<Success, PolicyViolationError, PolicyRestrictedError>> EvaluatedPortalPolicy(
        UserId? userId, PolicyType type, DateTime evaluatedAt, CancellationToken cancellationToken);

    Task<Result<Success, ForumNotFoundError, PolicyViolationError, PolicyRestrictedError>>
        EvaluatedForumPolicy(ForumId forumId, UserId? userId, PolicyType type, DateTime evaluatedAt,
            CancellationToken cancellationToken);

    Task<Result<Success, CategoryNotFoundError, PolicyViolationError, PolicyRestrictedError>>
        EvaluatedCategoryPolicy(CategoryId categoryId, UserId? userId, PolicyType type, DateTime evaluatedAt,
            CancellationToken cancellationToken);

    Task<Result<Success, ThreadNotFoundError, PolicyViolationError, PolicyRestrictedError>>
        EvaluatedThreadPolicy(ThreadId threadId, UserId? userId, PolicyType type, DateTime evaluatedAt,
            CancellationToken cancellationToken);
}