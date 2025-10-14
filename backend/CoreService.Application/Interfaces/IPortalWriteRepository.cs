using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IPortalWriteRepository
{
    Task<Portal> GetAsync(CancellationToken cancellationToken);
}