using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using Shared.Application.Abstractions;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface INotificationReadRepository
{
    public Task<Count> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken);

    public Task<PagedList<T>> GetAllAsync<T>(GetInternalNotificationsPagedQuery request, CancellationToken cancellationToken);
}