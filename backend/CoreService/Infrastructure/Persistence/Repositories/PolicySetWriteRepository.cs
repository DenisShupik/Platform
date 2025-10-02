using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;

namespace CoreService.Infrastructure.Persistence.Repositories;

public sealed class PolicySetWriteRepository : IPolicySetWriteRepository
{
    private readonly WriteApplicationDbContext _dbContext;

    public PolicySetWriteRepository(WriteApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ForumPolicySet forumPolicySet, CancellationToken cancellationToken)
    {
        await _dbContext.ForumPolicySets.AddAsync(forumPolicySet, cancellationToken);
    }
}