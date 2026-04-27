using System.Text.Json.Serialization;
using CoreService.Domain.Entities;
using CoreService.Domain.Events;
using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Enums;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Domain.Entities;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedNotifiableEventPayload), nameof(NotifiableEventPayloadType.PostAdded))]
[JsonDerivedType(typeof(PostUpdatedNotifiableEventPayload), nameof(NotifiableEventPayloadType.PostUpdated))]
[JsonDerivedType(typeof(ThreadApprovedNotifiableEventPayload), nameof(NotifiableEventPayloadType.ThreadApproved))]
[JsonDerivedType(typeof(ThreadRejectedNotifiableEventPayload), nameof(NotifiableEventPayloadType.ThreadRejected))]
public abstract class NotifiableEventPayload;

[Include(typeof(Post), PropertyGenerationMode.AsPrivateSet, nameof(Post.ThreadId), nameof(Post.PostId),
    nameof(Post.CreatedBy))]
public sealed partial class PostAddedNotifiableEventPayload : NotifiableEventPayload
{
    public PostAddedNotifiableEventPayload(ThreadId threadId, PostId postId, UserId createdBy)
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
    public PostUpdatedNotifiableEventPayload(ThreadId threadId, PostId postId, UserId updatedBy)
    {
        ThreadId = threadId;
        PostId = postId;
        UpdatedBy = updatedBy;
    }
}

[Omit(typeof(ThreadApprovedEvent), PropertyGenerationMode.AsPrivateSet)]
public sealed partial class ThreadApprovedNotifiableEventPayload : NotifiableEventPayload
{
    public ThreadApprovedNotifiableEventPayload(ThreadId threadId, UserId createdBy, UserId approvedBy,
        DateTime approvedAt)
    {
        ThreadId = threadId;
        CreatedBy = createdBy;
        ApprovedBy = approvedBy;
        ApprovedAt = approvedAt;
    }
}

[Omit(typeof(ThreadRejectedEvent), PropertyGenerationMode.AsPrivateSet)]
public sealed partial class ThreadRejectedNotifiableEventPayload : NotifiableEventPayload
{
    public ThreadRejectedNotifiableEventPayload(ThreadId threadId, UserId createdBy, UserId rejectedBy,
        DateTime rejectedAt)
    {
        ThreadId = threadId;
        CreatedBy = createdBy;
        RejectedBy = rejectedBy;
        RejectedAt = rejectedAt;
    }
}