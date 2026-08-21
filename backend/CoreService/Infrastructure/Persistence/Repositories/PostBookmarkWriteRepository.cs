using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using Shared.Domain.Abstractions.Results;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.ValueObjects;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostBookmarkWriteRepository : IPostBookmarkWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PostBookmarkWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SuccessOr<DuplicatePostBookmarkError>> ExecuteAddAsync(
        PostBookmark postBookmark,
        CancellationToken cancellationToken
    )
    {
        var insertedCount = await _dbContext.PostBookmarks
            .ToLinqToDBTable()
            .UpsertAsync(postBookmark, upsert => upsert.SkipUpdate(), cancellationToken);

        return insertedCount == 0
            ? new DuplicatePostBookmarkError(postBookmark.UserId, postBookmark.PostId)
            : SuccessOr.Success;
    }

    public async Task<SuccessOr<PostBookmarkNotFoundError>> ExecuteRemoveAsync(
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
            : SuccessOr.Success;
    }
}
