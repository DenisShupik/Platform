using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId))]
public sealed partial class GetInternalNotificationsPagedQuery : PagedQuery<PaginationLimitMin10Max100Default100,
    GetInternalNotificationsPagedQuery.GetInternalNotificationQuerySortType>
{
    public enum GetInternalNotificationQuerySortType
    {
        OccurredAt = 0,
        DeliveredAt = 1
    }

    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }
}

public sealed class GetInternalNotificationsPagedQueryHandler
{
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly ICoreServiceClient _coreServiceClient;
    private readonly IUserServiceClient _userServiceClient;

    public GetInternalNotificationsPagedQueryHandler(
        INotificationReadRepository notificationReadRepository,
        ICoreServiceClient coreServiceClient,
        IUserServiceClient userServiceClient
    )
    {
        _notificationReadRepository = notificationReadRepository;
        _coreServiceClient = coreServiceClient;
        _userServiceClient = userServiceClient;
    }

    public async Task<InternalNotificationsPagedDto> HandleAsync(GetInternalNotificationsPagedQuery query,
        CancellationToken cancellationToken)
    {
        var notificationPagedList =
            await _notificationReadRepository.GetAllAsync<InternalNotificationDto>(query, cancellationToken);

        var threadIds = new HashSet<ThreadId>();
        var userIds = new HashSet<UserId>();

        foreach (var payload in notificationPagedList.Items.Select(e => e.Payload))
        {
            switch (payload)
            {
                case PostAddedNotifiableEventPayload typedPayload:
                {
                    threadIds.Add(typedPayload.ThreadId);
                    userIds.Add(typedPayload.CreatedBy);
                }
                    break;
                case PostUpdatedNotifiableEventPayload typedPayload:
                {
                    threadIds.Add(typedPayload.ThreadId);
                    userIds.Add(typedPayload.UpdatedBy);
                }
                    break;
            }
        }

        var threadTask = _coreServiceClient.GetThreadsAsync(threadIds, cancellationToken).AsTask();
        var userTask = _userServiceClient.GetUsersAsync(userIds, cancellationToken).AsTask();

        await Task.WhenAll(threadTask, userTask);

        var threads = threadTask.Result.ToDictionary(e => e.ThreadId, e => e.Title);
        var users = userTask.Result.ToDictionary(e => e.UserId, e => e.Username);

        return new InternalNotificationsPagedDto
        {
            Notifications = notificationPagedList.Items,
            Threads = threads,
            Users = users,
            TotalCount = notificationPagedList.TotalCount
        };
    }
}