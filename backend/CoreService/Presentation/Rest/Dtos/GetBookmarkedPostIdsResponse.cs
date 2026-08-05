using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Rest.Dtos;

public sealed class GetBookmarkedPostIdsResponse
{
    public required PostId[] PostIds { get; init; }
}
