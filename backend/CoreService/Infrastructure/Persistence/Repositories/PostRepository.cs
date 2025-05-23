using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<PostNotFoundError, Post>> GetOneAsync(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken)
    {
        var forum = await _dbContext.Posts
            .Where(e => e.ThreadId == threadId && e.PostId == postId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (forum == null) return new PostNotFoundError(threadId, postId);

        return forum;
    }
}