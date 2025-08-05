using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using Generator.Attributes;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Abstractions;
using UserService.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

[Include(typeof(UserNotification), PropertyGenerationMode.AsRequired, nameof(UserNotification.UserId))]
public sealed partial class GetInternalUserNotificationQuery : PagedQuery
{
    public enum GetInternalUserNotificationQuerySortType
    {
        OccurredAt = 0,
        DeliveredAt = 1
    }

    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteriaList<GetInternalUserNotificationQuerySortType>? Sort { get; init; }
}

public sealed class
    GetInternalUserNotificationQueryValidator : PagedQueryValidator<GetInternalUserNotificationQuery>;

public sealed class GetInternalUserNotificationQueryHandler
{
    private readonly IUserNotificationReadRepository _userNotificationReadRepository;
    private readonly ICoreServiceClient _coreServiceClient;
    private readonly IUserServiceClient _userServiceClient;

    public GetInternalUserNotificationQueryHandler(
        IUserNotificationReadRepository userNotificationReadRepository,
        ICoreServiceClient coreServiceClient,
        IUserServiceClient userServiceClient
    )
    {
        _userNotificationReadRepository = userNotificationReadRepository;
        _coreServiceClient = coreServiceClient;
        _userServiceClient = userServiceClient;
    }

    public async Task<InternalUserNotificationsDto> HandleAsync(GetInternalUserNotificationQuery query,
        CancellationToken cancellationToken)
    {
        var userNotificationPagedList =
            await _userNotificationReadRepository.GetAllAsync<InternalUserNotificationDto>(query, cancellationToken);

        var threadIds = new HashSet<ThreadId>();
        var userIds = new HashSet<UserId>();

        foreach (var payload in userNotificationPagedList.Items.Select(e => e.Payload))
        {
            switch (payload)
            {
                case PostAddedNotificationPayload typedPayload:
                {
                    threadIds.Add(typedPayload.ThreadId);
                    userIds.Add(typedPayload.CreatedBy);
                }
                    break;
                case PostUpdatedNotificationPayload typedPayload:
                {
                    threadIds.Add(typedPayload.ThreadId);
                    userIds.Add(typedPayload.UpdatedBy);
                }
                    break;
            }
        }

        var threadTasks = threadIds.Select(e => _coreServiceClient.GetThreadAsync(e, cancellationToken).AsTask());
        var userTasks = userIds.Select(e => _userServiceClient.GetUserAsync(e, cancellationToken).AsTask());

        var threads = (await Task.WhenAll(threadTasks)).ToDictionary(e => e.ThreadId, e => e.Title);
        var users = (await Task.WhenAll(userTasks)).ToDictionary(e => e.UserId, e => e.Username);

        return new InternalUserNotificationsDto
        {
            Notifications = userNotificationPagedList.Items,
            Threads = threads,
            Users = users,
            TotalCount = userNotificationPagedList.TotalCount
        };
    }
}