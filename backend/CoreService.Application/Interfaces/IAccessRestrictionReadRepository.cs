using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<Result<Success, PolicyViolationError, AccessPolicyRestrictedError>> CheckUserAccessAsync(
        UserId? userId,
        ForumId forumId, CancellationToken cancellationToken);

    Task<Result<Success, PolicyViolationError, AccessPolicyRestrictedError>>
        CheckUserAccessAsync(UserId? userId, CategoryId categoryId, CancellationToken cancellationToken);

    Task<Result<Success, PolicyViolationError, PolicyRestrictedError>> CheckUserAccessAsync(UserId? userId,
        PostId postId,
        CancellationToken cancellationToken);

    Task<Result<Success, ForumNotFoundError, PolicyViolationError, AccessPolicyRestrictedError, CategoryCreatePolicyRestrictedError>>
        CheckUserCanCreateCategoryAsync(UserId? userId, ForumId forumId, DateTime timestamp,
            CancellationToken cancellationToken);

    Task<Result<Success, CategoryNotFoundError, PolicyViolationError, AccessPolicyRestrictedError,ThreadCreatePolicyRestrictedError>>
        CanUserCanCreateThreadAsync(UserId? userId, CategoryId categoryId, DateTime timestamp,
            CancellationToken cancellationToken);

    Task<Result<Success, ThreadNotFoundError, PolicyViolationError, AccessPolicyRestrictedError, PostCreatePolicyRestrictedError>>
        CheckUserCanCreatePostAsync(UserId? userId, ThreadId threadId, DateTime timestamp,
            CancellationToken cancellationToken);
}