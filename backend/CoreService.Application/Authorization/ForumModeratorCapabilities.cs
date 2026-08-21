using CoreService.Domain.Enums;

namespace CoreService.Application.Authorization;

/// <summary>
/// Набор полномочий, выдаваемый назначением модератора форума.
/// </summary>
public static class ForumModeratorCapabilities
{
    public static readonly IReadOnlyList<CapabilityCode> All =
    [
        CapabilityCode.ViewUnpublishedThreads,
        CapabilityCode.ApproveThreads,
        CapabilityCode.RejectThreads,
        CapabilityCode.EditAnyPost,
        CapabilityCode.DeleteAnyPost
    ];
}
