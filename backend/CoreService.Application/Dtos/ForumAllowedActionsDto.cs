namespace CoreService.Application.Dtos;

public sealed record ForumAllowedActionsDto
{
    public required bool CanManageStructure { get; init; }
    public required bool CanManageAuthorization { get; init; }
    public required bool CanManageSanctions { get; init; }
}
