using System.Text.Json.Serialization;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Enums;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedNotifiableEventPayload), nameof(NotifiableEventPayloadType.PostAdded))]
[JsonDerivedType(typeof(PostUpdatedNotifiableEventPayload), nameof(NotifiableEventPayloadType.PostUpdated))]
public abstract class NotifiableEventPayload;

[Include(typeof(Post), PropertyGenerationMode.AsPrivateSet, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.CreatedBy))]
public sealed partial class PostAddedNotifiableEventPayload : NotifiableEventPayload
{
    public PostAddedNotifiableEventPayload(ThreadId threadId, PostId postId, UserId? createdBy)
    {
        ThreadId = threadId;
        PostId = postId;
        CreatedBy = createdBy;
    }
}

[Include(typeof(Post), PropertyGenerationMode.AsPrivateSet, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.UpdatedBy))]
public sealed partial class PostUpdatedNotifiableEventPayload : NotifiableEventPayload
{
    public PostUpdatedNotifiableEventPayload(ThreadId threadId, PostId postId, UserId? updatedBy)
    {
        ThreadId = threadId;
        PostId = postId;
        UpdatedBy = updatedBy;
    }
}