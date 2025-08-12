using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using SharedKernel.Application.Abstractions;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface INotificationReadRepository
{
    public Task<int> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken);

    public Task<PagedList<T>> GetAllAsync<T>(GetInternalNotificationsPagedQuery request, CancellationToken cancellationToken);
}