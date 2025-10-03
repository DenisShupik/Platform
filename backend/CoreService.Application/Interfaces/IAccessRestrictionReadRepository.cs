using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<Result<Success, ForumAccessPolicyViolationError, ForumPolicyRestrictedError>> CheckUserAccessAsync(UserId? userId,
        ForumId forumId, CancellationToken cancellationToken);

    Task<Result<Success, ForumAccessPolicyViolationError, CategoryAccessPolicyViolationError, ForumPolicyRestrictedError,
        CategoryPolicyRestrictedError>> CheckUserAccessAsync(UserId? userId, CategoryId categoryId,
        CancellationToken cancellationToken);

    Task<Result<Success, AccessPolicyViolationError, PolicyRestrictedError>> CheckUserAccessAsync(UserId? userId, PostId postId,
        CancellationToken cancellationToken);

    Task<Result<Success, ForumNotFoundError, CategoryCreatePolicyViolationError>>
        CheckUserCanCreateCategoryAsync(UserId userId, ForumId forumId, CancellationToken cancellationToken);

    Task<Result<Success, CategoryNotFoundError, ForumAccessPolicyViolationError, ForumPolicyRestrictedError,
        CategoryAccessPolicyViolationError, CategoryPolicyRestrictedError, ThreadCreatePolicyViolationError>> CanUserCanCreateThreadAsync(
        UserId userId,
        CategoryId categoryId, CancellationToken cancellationToken);

    Task<Result<Success, ThreadNotFoundError, AccessPolicyViolationError, PolicyRestrictedError, PostCreatePolicyViolationError>>
        CheckUserCanCreatePostAsync(UserId userId, ThreadId threadId, DateTime timestamp, CancellationToken cancellationToken);
}