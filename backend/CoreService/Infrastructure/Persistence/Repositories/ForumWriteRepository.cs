using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using LinqToDB.EntityFrameworkCore;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class ForumWriteRepository : IForumWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public ForumWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<T, ForumNotFoundError>> GetAsync<T>(ForumId forumId, CancellationToken cancellationToken)
        where T : class, IHasForumId
    {
        var forum = await _dbContext.Set<T>()
            .Where(e => e.ForumId == forumId)
            .FirstOrDefaultAsyncEF(cancellationToken);

        if (forum == null) return new ForumNotFoundError();

        return forum;
    }

    public void Add(Forum forum)
    {
        _dbContext.Forums.Add(forum);
    }
}