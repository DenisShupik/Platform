using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class AccessWriteRepository : IAccessWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public AccessWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken)
    {
        await _dbContext.Policies.AddAsync(policy, cancellationToken);
    }
    
    public async Task AddRangeAsync(IEnumerable<Policy> policies, CancellationToken cancellationToken)
    {
        await _dbContext.Policies.AddRangeAsync(policies, cancellationToken);
    }

    public async Task AddAsync(Grant grant, CancellationToken cancellationToken)
    {
        await _dbContext.Grants.AddAsync(grant, cancellationToken);
    }
}