using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Persistence.Extensions;

public static class NotifiableEventPayloadExtensions
{
    public static bool IsPostEventFor(this NotifiableEventPayload payload, ThreadId threadId) =>
        payload is PostAddedNotifiableEventPayload { ThreadId: var postAddedThreadId } &&
        postAddedThreadId == threadId ||
        payload is PostUpdatedNotifiableEventPayload { ThreadId: var postUpdatedThreadId } &&
        postUpdatedThreadId == threadId;
}
