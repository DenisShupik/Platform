using CoreService.Domain.Entities;

namespace CoreService.Application.Interfaces;

public interface IAccessWriteRepository
{
    Task AddAsync(Policy policy, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<Policy> policies, CancellationToken cancellationToken);
    Task AddAsync(Grant grant, CancellationToken cancellationToken);
}