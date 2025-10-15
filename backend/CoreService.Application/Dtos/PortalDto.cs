using CoreService.Domain.Enums;

namespace CoreService.Application.Dtos;

public sealed class PortalDto
{
    public required PolicyValue ReadPolicy { get; init; }
    public required PolicyValue ForumCreatePolicy { get; init; }
    public required PolicyValue CategotyCreatePolicy { get; init; }
    public required PolicyValue ThreadCreatePolicy { get; init; }
    public required PolicyValue PostCreatePolicy { get; init; }
}