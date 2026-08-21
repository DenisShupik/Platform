namespace CoreService.Application.Dtos;

public sealed record ThreadAllowedActionsDto
{
    public required bool CanViewUnpublishedThreads { get; init; }
    public required bool CanApproveThread { get; init; }
    public required bool CanRejectThread { get; init; }
    public required bool CanEditAnyPost { get; init; }
    public required bool CanDeleteAnyPost { get; init; }
    public required bool CanManageAuthorization { get; init; }
    public required bool CanManageSanctions { get; init; }
}
