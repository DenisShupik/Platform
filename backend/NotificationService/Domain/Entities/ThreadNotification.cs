using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using NotificationService.Domain.Enums;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace NotificationService.Domain.Entities;

[Include(typeof(User), nameof(User.UserId))]
[Include(typeof(Thread), nameof(Thread.ThreadId))]
public sealed partial class ThreadNotification
{
    /// <summary>
    /// Канал доставки оповещения
    /// </summary>
    public ChannelType Channel { get; private set; }

    /// <summary>
    /// Дата и время доставки оповещения
    /// </summary>
    public DateTime? DeliveredAt { get; private set; }

    public ThreadNotification(UserId userId, ThreadId threadId, ChannelType channel)
    {
        UserId = userId;
        ThreadId = threadId;
        Channel = channel;
        DeliveredAt = null;
    }
}