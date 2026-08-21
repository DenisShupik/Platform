namespace CoreService.Application.Dtos;

public sealed record AdministrationAllowedActionsDto
{
    public required bool CanManageAnyAuthorization { get; init; }
    public required bool CanManageAnySanctions { get; init; }
    public required bool CanManagePlatformAuthorization { get; init; }
    public required bool CanManagePlatformSanctions { get; init; }
}
