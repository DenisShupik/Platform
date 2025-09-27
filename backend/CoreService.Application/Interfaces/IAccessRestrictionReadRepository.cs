using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using OneOf.Types;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<OneOf<Success, AccessRestrictedError>> CanUserPostInThreadAsync(UserId userId, ThreadId threadId,
        CancellationToken cancellationToken);

    Task<OneOf<Success, ForumAccessLevelError, ForumAccessRestrictedError>> CheckUserAccessAsync(UserId? userId, ForumId forumId,
        CancellationToken cancellationToken);

    Task<OneOf<Success, AccessLevelError, AccessRestrictedError>> CheckUserAccessAsync(UserId? userId, PostId postId,
        CancellationToken cancellationToken);
}