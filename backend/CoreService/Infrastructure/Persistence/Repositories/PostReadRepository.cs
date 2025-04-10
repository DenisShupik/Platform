using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using LinqToDB.EntityFrameworkCore;
using Mapster;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostReadRepository : IPostReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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