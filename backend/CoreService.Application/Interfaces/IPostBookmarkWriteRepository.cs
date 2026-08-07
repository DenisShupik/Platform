using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPostBookmarkWriteRepository
{
    void Add(PostBookmark postBookmark);

    Task<Result<Success, PostBookmarkNotFoundError>> ExecuteRemoveAsync(
        UserId userId,
        PostId postId,
        CancellationToken cancellationToken
    );
}
