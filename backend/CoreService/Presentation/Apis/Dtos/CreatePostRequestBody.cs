using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Apis.Dtos;

public sealed class CreatePostRequestBody
{
    public required PostContent Content { get; init; }
}