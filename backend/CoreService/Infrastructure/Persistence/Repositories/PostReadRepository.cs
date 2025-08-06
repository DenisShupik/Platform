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
    private readonly ReadonlyApplicationDbContext _dbContext;

    private sealed class Projection<T>
    {
        public T? Post { get; set; }
    }

    public PostReadRepository(ReadonlyApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ThreadNotFoundError, PostNotFoundError>> GetOneAsync<T>(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken)
    {
        var query = await (
                from t in _dbContext.Threads
                join p in _dbContext.Posts on t.ThreadId equals p.ThreadId into g
                from p in g.DefaultIfEmpty()
                where t.ThreadId == threadId && p.PostId == postId
                select new { Post = p }
            )
            .ProjectToType<Projection<T>>()
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (query == null) return new ThreadNotFoundError(threadId);
        if (query.Post == null) return new PostNotFoundError(threadId, postId);
        return query.Post;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var query = await _dbContext.Posts
            .OrderBy(e => e.PostId)
            .Where(x => request.ThreadId == null || x.ThreadId == request.ThreadId)
            .Skip(request.Offset)
            .Take(request.Limit)
            .ProjectToType<T>()
            .ToListAsyncEF(cancellationToken);

        return query;
    }
}