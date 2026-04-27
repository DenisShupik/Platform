using NotificationService.Domain.Entities;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.Entities;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsPublic, nameof(ThreadSubscription.ThreadId))]
public sealed partial class WatchedThreadLatestEvent
{
    public NotifiableEvent LatestEvent { get; set; }
}