using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumRepository : IForumRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ForumRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Forum forum, CancellationToken cancellationToken)
    {
        await _dbContext.Forums.AddAsync(forum, cancellationToken);
    }
}