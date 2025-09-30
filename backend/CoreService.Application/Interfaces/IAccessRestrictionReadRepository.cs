using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<Result<Success, ForumAccessLevelError, ForumAccessRestrictedError>> CheckUserAccessAsync(UserId? userId,
        ForumId forumId, CancellationToken cancellationToken);

    Task<Result<Success, ForumAccessLevelError, CategoryAccessLevelError, ForumAccessRestrictedError,
        CategoryAccessRestrictedError>> CheckUserAccessAsync(UserId? userId, CategoryId categoryId,
        CancellationToken cancellationToken);

    Task<Result<Success, AccessLevelError, AccessRestrictedError>> CheckUserAccessAsync(UserId? userId, PostId postId,
        CancellationToken cancellationToken);

    Task<Result<Success, ForumNotFoundError, ForumModerationForbiddenError>>
        CheckUserCanCreateCategoryAsync(UserId userId, ForumId forumId, CancellationToken cancellationToken);

    Task<Result<Success, CategoryNotFoundError, AccessLevelError, AccessRestrictedError>> CheckUserWriteAccessAsync(
        UserId userId,
        CategoryId categoryId, CancellationToken cancellationToken);

    Task<Result<Success, ThreadNotFoundError, AccessLevelError, AccessRestrictedError, PostCreatePolicyViolationError>>
        CheckUserCanCreatePostAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
}