using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

public enum GetInternalNotificationsPagedQuerySortType : byte
{
    OccurredAt = 0,
    DeliveredAt = 1
}

[Include(typeof(Notification), PropertyGenerationMode.AsRequired, nameof(Notification.UserId))]
public sealed partial class GetInternalNotificationsPagedQuery : MultiSortPagedQuery<InternalNotificationsPagedDto,
    GetInternalNotificationsPagedQuerySortType>
{
    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }
}

public sealed class
    GetInternalNotificationsPagedQueryHandler : IQueryHandler<GetInternalNotificationsPagedQuery,
    InternalNotificationsPagedDto>
{
    private readonly INotificationReadRepository _notificationReadRepository;
    private readonly IThreadAccessReader _threadAccessReader;
    private readonly IUserDirectoryReader _userDirectoryReader;

    public GetInternalNotificationsPagedQueryHandler(
        INotificationReadRepository notificationReadRepository,
        IThreadAccessReader threadAccessReader,
        IUserDirectoryReader userDirectoryReader
    )
    {
        _notificationReadRepository = notificationReadRepository;
        _threadAccessReader = threadAccessReader;
        _userDirectoryReader = userDirectoryReader;
    }

    public async Task<InternalNotificationsPagedDto> HandleAsync(GetInternalNotificationsPagedQuery query,
        CancellationToken cancellationToken)
    {
        var threadIds = await _notificationReadRepository.GetThreadIdsAsync(
            query.UserId,
            query.IsDelivered,
            Domain.Enums.ChannelType.Internal,
            cancellationToken);
        var readableThreads = await _threadAccessReader.GetReadableAsync(
            threadIds, query.UserId, cancellationToken);
        var threads = readableThreads.ToDictionary(thread => thread.ThreadId, thread => thread.Title);
        var readableThreadIds = threads.Keys.ToHashSet();
        var notificationPagedList = await _notificationReadRepository.GetAllAsync<InternalNotificationDto>(
            query,
            readableThreadIds,
            cancellationToken);
        var visibleNotifications = notificationPagedList.Items;

        var userIds = new HashSet<UserId>();
        foreach (var payload in visibleNotifications.Select(e => e.Payload))
        {
            switch (payload)
            {
                case PostAddedNotifiableEventPayload typedPayload:
                    {
                        userIds.Add(typedPayload.CreatedBy);
                    }
                    break;
                case PostUpdatedNotifiableEventPayload typedPayload:
                    {
                        userIds.Add(typedPayload.UpdatedBy);
                    }
                    break;
                case ThreadApprovedNotifiableEventPayload typedPayload:
                    {
                        userIds.Add(typedPayload.ApprovedBy);
                    }
                    break;
                case ThreadRejectedNotifiableEventPayload typedPayload:
                    {
                        userIds.Add(typedPayload.RejectedBy);
                    }
                    break;
            }
        }

        var users = (await _userDirectoryReader.GetUsersAsync(userIds, cancellationToken))
            .ToDictionary(e => e.UserId, e => e.Username);

        return new InternalNotificationsPagedDto
        {
            Notifications = visibleNotifications,
            Threads = threads,
            Users = users,
            TotalCount = notificationPagedList.TotalCount
        };
    }
}
