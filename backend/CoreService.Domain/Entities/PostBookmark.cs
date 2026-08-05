using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Domain.Entities;

[Include(typeof(Post), PropertyGenerationMode.AsPrivateSet, nameof(Post.PostId))]
public sealed partial class PostBookmark
{
    public UserId UserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public PostBookmark(UserId userId, PostId postId, DateTime createdAt)
    {
        UserId = userId;
        PostId = postId;
        CreatedAt = createdAt;
    }
}
