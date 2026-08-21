using CoreService.Domain.Enums;

namespace CoreService.Application.Authorization;

/// <summary>
/// Явно ревьюимый набор полномочий платформенного администратора.
/// Новые capabilities не добавляются к назначению автоматически.
/// </summary>
public static class PlatformAdministratorCapabilities
{
    public static readonly IReadOnlyList<CapabilityCode> All =
    [
        CapabilityCode.ManageStructure,
        CapabilityCode.ViewUnpublishedThreads,
        CapabilityCode.ApproveThreads,
        CapabilityCode.RejectThreads,
        CapabilityCode.EditAnyPost,
        CapabilityCode.DeleteAnyPost,
        CapabilityCode.ManageAuthorization,
        CapabilityCode.ManageSanctions
    ];
}
