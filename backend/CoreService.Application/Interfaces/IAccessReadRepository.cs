using CoreService.Application.UseCases;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IAccessReadRepository
{
    Task<Dictionary<PolicyType, bool>> GetPortalPermissionsAsync(GetPortalPermissionsQuery query,
        CancellationToken cancellationToken);

    Task<Result<Dictionary<PolicyType, bool>, ForumNotFoundError>> GetForumPermissionsAsync(
        GetForumPermissionsQuery query, CancellationToken cancellationToken);
}