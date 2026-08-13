using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPostBookmarkWriteRepository
{
    Task<SuccessOr<DuplicatePostBookmarkError>> ExecuteAddAsync(
        PostBookmark postBookmark,
        CancellationToken cancellationToken
    );

    Task<SuccessOr<PostBookmarkNotFoundError>> ExecuteRemoveAsync(
        UserId userId,
        PostId postId,
        CancellationToken cancellationToken
    );
}
