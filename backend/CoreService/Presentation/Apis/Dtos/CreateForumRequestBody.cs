using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Apis.Dtos;

public sealed class CreateForumRequestBody
{
    public required ForumTitle Title { get; init; }
}