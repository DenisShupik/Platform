using NotificationService.Application.UseCases;
using NotificationService.Domain.Enums;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IUserNotificationReadRepository
{
    public Task<int> GetCountAsync(UserId userId, bool? isDelivered, ChannelType? channel,
        CancellationToken cancellationToken);

    public Task<IReadOnlyList<T>> GetAllAsync<T>(GetInternalUserNotificationQuery request, CancellationToken cancellationToken);
}