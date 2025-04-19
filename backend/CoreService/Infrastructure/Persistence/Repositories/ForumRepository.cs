using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using OneOf;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumRepository : IForumRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ForumRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<T, ForumNotFoundError>> GetAsync<T>(ForumId forumId, CancellationToken cancellationToken)
        where T : class, IHasForumId
    {
        var forum = await _dbContext.Set<T>()
            .Where(e => e.ForumId == forumId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (forum == null) return new ForumNotFoundError(forumId);

        return forum;
    }

    public async Task AddAsync(Forum forum, CancellationToken cancellationToken)
    {
        await _dbContext.Forums.AddAsync(forum, cancellationToken);
    }
}