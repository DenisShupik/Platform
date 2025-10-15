using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using LinqToDB.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PortalReadRepository : IPortalReadRepository
{
    private readonly ReadApplicationDbContext _dbContext;

    public PortalReadRepository(ReadApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<PortalDto> GetAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Portal
            .Where(e => e.PortalId == 1)
            .Select(e => new PortalDto
            {
                ReadPolicy = e.ReadPolicy.Value,
                ForumCreatePolicy = e.ForumCreatePolicy.Value,
                CategotyCreatePolicy = e.CategoryCreatePolicy.Value,
                ThreadCreatePolicy = e.ThreadCreatePolicy.Value,
                PostCreatePolicy = e.PostCreatePolicy.Value,
            })
            .SingleAsyncEF(cancellationToken);
    }
}