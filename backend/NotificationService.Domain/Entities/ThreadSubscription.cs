using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Enums;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace NotificationService.Domain.Entities;

[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
[Include(typeof(Thread), PropertyGenerationMode.AsPrivateSet, nameof(Thread.ThreadId))]
public sealed partial class ThreadSubscription
{
    /// <summary>
    /// Каналы, по которым пользователь подписан на уведомления по данной теме
    /// </summary>
    public EnumSet<ChannelType> Channels { get; private set; }

    public ThreadSubscription(UserId userId, ThreadId threadId, EnumSet<ChannelType> channels)
    {
        UserId = userId;
        ThreadId = threadId;
        Channels = channels;
    }
}