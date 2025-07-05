using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace NotificationService.Domain.Entities;

[Include(typeof(User), nameof(User.UserId))]
[Include(typeof(Thread), nameof(Thread.ThreadId))]
public sealed partial class ThreadSubscription
{
    public ThreadSubscription(UserId userId, ThreadId threadId)
    {
        UserId = userId;
        ThreadId = threadId;
    }
}