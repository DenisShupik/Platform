using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PostRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken)
    {
        await _dbContext.Posts.AddAsync(post, cancellationToken);
    }
}