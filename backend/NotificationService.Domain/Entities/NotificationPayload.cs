using System.Text.Json.Serialization;
using CoreService.Domain.Entities;
using Generator.Attributes;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedNotificationPayload), (int)NotificationPayloadType.PostAdded)]
[JsonDerivedType(typeof(PostUpdatedNotificationPayload), (int)NotificationPayloadType.PostUpdated)]
public abstract class NotificationPayload;

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.CreatedBy))]
public sealed partial class PostAddedNotificationPayload : NotificationPayload;

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId), nameof(Post.UpdatedBy))]
public sealed partial class PostUpdatedNotificationPayload : NotificationPayload;