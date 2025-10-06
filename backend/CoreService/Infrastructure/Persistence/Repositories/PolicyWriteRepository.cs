using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PolicyWriteRepository : IPolicyWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PolicyWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Policy policy, CancellationToken cancellationToken)
    {
        await _dbContext.Policies.AddAsync(policy, cancellationToken);
    }
}