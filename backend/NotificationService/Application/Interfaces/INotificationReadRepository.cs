using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface INotificationReadRepository
{
    Task<IReadOnlySet<ThreadId>> GetThreadIdsAsync(
        UserId userId,
        bool? isDelivered,
        ChannelType? channel,
        CancellationToken cancellationToken);

    public Task<Count> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken);

    public Task<PagedList<T>> GetAllAsync<T>(
        GetInternalNotificationsPagedQuery request,
        IReadOnlySet<ThreadId> readableThreadIds,
        CancellationToken cancellationToken);
}
