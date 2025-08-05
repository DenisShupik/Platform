using System.Text.Json.Serialization;
using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using NotificationService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace NotificationService.Domain.Entities;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedNotificationPayload), (int)NotificationPayloadType.PostAdded)]
[JsonDerivedType(typeof(PostUpdatedNotificationPayload), (int)NotificationPayloadType.PostUpdated)]
public abstract class NotificationPayload;

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.CreatedBy))]
public sealed partial class PostAddedNotificationPayload : NotificationPayload
{
    public PostAddedNotificationPayload(ThreadId threadId, PostId postId, UserId createdBy)
    {
        ThreadId = threadId;
        PostId = postId;
        CreatedBy = createdBy;
    }
}

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.UpdatedBy))]
public sealed partial class PostUpdatedNotificationPayload : NotificationPayload
{
    public PostUpdatedNotificationPayload(ThreadId threadId, PostId postId, UserId updatedBy)
    {
        ThreadId = threadId;
        PostId = postId;
        UpdatedBy = updatedBy;
    }
}