using CoreService.Domain.Entities;

namespace CoreService.Application.Interfaces;

public interface IPolicyWriteRepository
{
    Task AddAsync(Policy policy, CancellationToken cancellationToken);
}