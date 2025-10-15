using CoreService.Domain.Entities;

namespace CoreService.Application.Interfaces;

public interface IPortalWriteRepository
{
    Task<Portal> GetAsync(CancellationToken cancellationToken);
}