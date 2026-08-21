namespace CoreService.Application.Dtos;

public sealed record CategoryAllowedActionsDto
{
    public required bool CanManageStructure { get; init; }
    public required bool CanViewUnpublishedThreads { get; init; }
    public required bool CanApproveThread { get; init; }
    public required bool CanRejectThread { get; init; }
    public required bool CanEditAnyPost { get; init; }
    public required bool CanDeleteAnyPost { get; init; }
    public required bool CanManageModerators { get; init; }
    public required bool CanManageSanctions { get; init; }
}
