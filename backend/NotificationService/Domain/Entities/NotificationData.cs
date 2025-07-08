using System.Text.Json.Serialization;
using CoreService.Domain.Entities;
using Generator.Attributes;
using NotificationService.Domain.Enums;

namespace NotificationService.Domain.Entities;

[JsonPolymorphic]
[JsonDerivedType(typeof(PostAddedNotificationData), nameof(NotificationDataType.PostAdded))]
[JsonDerivedType(typeof(PostUpdatedNotificationData), nameof(NotificationDataType.PostUpdated))]
public abstract class NotificationData;

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId))]
public sealed partial class PostAddedNotificationData : NotificationData;

[Include(typeof(Post), nameof(Post.ThreadId), nameof(Post.PostId))]
public sealed partial class PostUpdatedNotificationData : NotificationData;