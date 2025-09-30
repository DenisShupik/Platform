using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostWriteRepository : IPostWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PostWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Post, PostNotFoundError>> GetOneAsync(PostId postId, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .Where(e => e.PostId == postId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (post == null) return new PostNotFoundError(postId);

        return post;
    }
}