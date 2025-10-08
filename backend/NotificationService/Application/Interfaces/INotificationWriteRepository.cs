using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Errors;
using NotificationService.Domain.ValueObjects;
using UserService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace NotificationService.Application.Interfaces;

public interface INotificationWriteRepository
{
    public Task<Result<Notification, NotificationNotFoundError>> GetOneAsync(UserId userId,
        NotifiableEventId notifiableEventId, ChannelType channel, CancellationToken cancellationToken);

    public Task BulkAddAsync(NotifiableEventId notifiableEventId, ThreadId threadId, UserId? userId,
        CancellationToken cancellationToken);

    public Task<Result<Success, NotificationNotFoundError>> ExecuteRemoveAsync(UserId userId,
        NotifiableEventId notifiableEventId, ChannelType channel, CancellationToken cancellationToken);
}