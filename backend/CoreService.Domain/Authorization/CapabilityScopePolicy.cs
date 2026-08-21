using CoreService.Domain.Enums;

namespace CoreService.Domain.Authorization;

/// <summary>
/// Defines the resource levels at which an atomic capability has business meaning.
/// This is a domain rule shared by command validation and permission-catalog projections.
/// </summary>
public static class CapabilityScopePolicy
{
    public static bool IsAllowed(CapabilityCode capability, AuthorizationScopeType scopeType) => capability switch
    {
        CapabilityCode.ManageStructure => scopeType is
            AuthorizationScopeType.Platform or AuthorizationScopeType.Forum,
        CapabilityCode.ViewUnpublishedThreads or
        CapabilityCode.ApproveThreads or
        CapabilityCode.RejectThreads or
        CapabilityCode.EditAnyPost or
        CapabilityCode.DeleteAnyPost or
        CapabilityCode.ManageAuthorization or
        CapabilityCode.ManageSanctions => scopeType is
            AuthorizationScopeType.Platform or
            AuthorizationScopeType.Forum or
            AuthorizationScopeType.Category or
            AuthorizationScopeType.Thread,
        _ => false
    };

    public static IReadOnlyList<AuthorizationScopeType> GetAllowedScopes(CapabilityCode capability) =>
        Enum.GetValues<AuthorizationScopeType>()
            .Where(scopeType => IsAllowed(capability, scopeType))
            .ToArray();
}
