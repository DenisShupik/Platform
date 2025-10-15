using CoreService.Application.Dtos;

namespace CoreService.Application.Interfaces;

public interface IPortalReadRepository
{
    Task<PortalDto> GetAsync(CancellationToken cancellationToken);
}