using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using LinqToDB.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PortalWriteRepository : IPortalWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PortalWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Portal> GetAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Portal
            .Where(e => e.PortalId == 1)
            .Include(e => e.ReadPolicy)
            .Include(e => e.ForumCreatePolicy)
            .Include(e => e.CategoryCreatePolicy)
            .Include(e => e.ThreadCreatePolicy)
            .Include(e => e.PostCreatePolicy)
            .SingleAsyncEF(cancellationToken);
    }
}