using CoreService.Application.Interfaces;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Extensions;
using LinqToDB;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.EntityFrameworkCore;
using Mapster;
using SharedKernel.Extensions;

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