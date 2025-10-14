using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IAccessRestrictionReadRepository
{
    Task<Result<Success, ForumNotFoundError, PolicyViolationError, ReadPolicyRestrictedError,
            CategoryCreatePolicyRestrictedError>>
        CheckUserCanCreateCategoryAsync(UserId? userId, ForumId forumId, DateTime timestamp,
            CancellationToken cancellationToken);

    Task<Result<Success, CategoryNotFoundError, PolicyViolationError, ReadPolicyRestrictedError,
            ThreadCreatePolicyRestrictedError>>
        CanUserCanCreateThreadAsync(UserId? userId, CategoryId categoryId, DateTime timestamp,
            CancellationToken cancellationToken);

    Task<Result<Success, ThreadNotFoundError, PolicyViolationError, ReadPolicyRestrictedError,
            PostCreatePolicyRestrictedError>>
        CheckUserCanCreatePostAsync(UserId? userId, ThreadId threadId, DateTime timestamp,
            CancellationToken cancellationToken);
}