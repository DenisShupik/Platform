using CoreService.Application.UseCases;
using CoreService.Domain.Enums;

namespace CoreService.Application.Interfaces;

public interface IAccessReadRepository
{
    Task<Dictionary<PolicyType,bool>> GetPortalPermissionsAsync(GetUserPortalPermissionsQuery query, CancellationToken cancellationToken);
}