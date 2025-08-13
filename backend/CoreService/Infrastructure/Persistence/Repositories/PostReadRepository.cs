using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    private sealed class Projection<T>
    {
        public T? Post { get; set; }
    }

    public PostReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, PostNotFoundError>> GetOneAsync<T>(PostId postId, CancellationToken cancellationToken)
    {
        var post = await _dbContext.Posts
            .Where(e => e.PostId == postId)
            .ProjectToType<T>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (post == null) return new PostNotFoundError(postId);

        return post;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetThreadPostsPagedQuery request,
        CancellationToken cancellationToken)
    {
        var query = await _dbContext.Posts
            .OrderBy(e => new { e.CreatedAt, e.PostId })
            .Where(e => e.ThreadId == request.ThreadId)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return query;
    }
}