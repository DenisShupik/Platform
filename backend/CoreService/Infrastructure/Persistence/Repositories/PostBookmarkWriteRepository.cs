using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostBookmarkWriteRepository : IPostBookmarkWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PostBookmarkWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(PostBookmark postBookmark)
    {
        _dbContext.PostBookmarks.Add(postBookmark);
    }

    public async Task<Result<Success, PostBookmarkNotFoundError>> ExecuteRemoveAsync(
        UserId userId,
        PostId postId,
        CancellationToken cancellationToken
    )
    {
        var deletedCount = await _dbContext.PostBookmarks
            .Where(e => e.UserId == userId && e.PostId == postId)
            .ExecuteDeleteAsync(cancellationToken);

        return deletedCount == 0
            ? new PostBookmarkNotFoundError(userId, postId)
            : Success.Instance;
    }
}
