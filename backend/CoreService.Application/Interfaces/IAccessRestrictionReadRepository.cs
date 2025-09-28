using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<Result<Success, AccessRestrictedError>> CanUserPostInThreadAsync(UserId userId, ThreadId threadId,
        CancellationToken cancellationToken);

    Task<Result<Success, ForumAccessLevelError, ForumAccessRestrictedError>> CheckUserAccessAsync(UserId? userId,
        ForumId forumId,
        CancellationToken cancellationToken);

    Task<Result<Success, ForumAccessLevelError, CategoryAccessLevelError, ForumAccessRestrictedError,
        CategoryAccessRestrictedError>> CheckUserAccessAsync(UserId? userId, CategoryId categoryId,
        CancellationToken cancellationToken);

    Task<Result<Success, AccessLevelError, AccessRestrictedError>> CheckUserAccessAsync(UserId? userId, PostId postId,
        CancellationToken cancellationToken);
}