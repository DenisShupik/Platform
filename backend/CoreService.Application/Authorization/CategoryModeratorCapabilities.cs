using CoreService.Domain.Enums;

namespace CoreService.Application.Authorization;

public static class CategoryModeratorCapabilities
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
