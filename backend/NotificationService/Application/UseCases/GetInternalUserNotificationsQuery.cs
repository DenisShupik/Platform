using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using Generator.Attributes;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using SharedKernel.Application.Abstractions;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Client;

namespace NotificationService.Application.UseCases;

[IncludeAsRequired(typeof(UserNotification), nameof(UserNotification.UserId))]
public sealed partial class GetInternalUserNotificationQuery : PaginatedQuery
{
    public enum SortType
    {
        DeliveryAt
    }

    /// <summary>
    /// Фильтр по статусу доставки
    /// </summary>
    public required bool? IsDelivered { get; init; }

    /// <summary>
    /// Фильтр по типу канала доставки уведомления
    /// </summary>
    public required ChannelType? Channel { get; init; }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<SortType>? Sort { get; init; }
}

public sealed class
    GetInternalUserNotificationQueryValidator : PaginatedQueryValidator<GetInternalUserNotificationQuery>;

public sealed class GetInternalUserNotificationQueryHandler
{
    private readonly IUserNotificationReadRepository _userNotificationReadRepository;
    private readonly CoreServiceGrpcClient _coreServiceGrpcClient;
    private readonly UserServiceGrpcClient _userServiceGrpcClient;

    public GetInternalUserNotificationQueryHandler(
        IUserNotificationReadRepository userNotificationReadRepository,
        CoreServiceGrpcClient coreServiceGrpcClient,
        UserServiceGrpcClient userServiceGrpcClient
    )
    {
        _userNotificationReadRepository = userNotificationReadRepository;
        _coreServiceGrpcClient = coreServiceGrpcClient;
        _userServiceGrpcClient = userServiceGrpcClient;
    }

    public async Task<InternalUserNotificationsDto> HandleAsync(GetInternalUserNotificationQuery query,
        CancellationToken cancellationToken)
    {
        var userNotification =
            await _userNotificationReadRepository.GetAllAsync<InternalUserNotificationDto>(query, cancellationToken);

        var threadIds = new HashSet<ThreadId>();
        var userIds = new HashSet<UserId>();

        foreach (var payload in userNotification.Select(e => e.Payload))
        {
            switch (payload)
            {
                case PostAddedNotificationPayload typedPayload:
                {
                    threadIds.Add(typedPayload.ThreadId);
                    userIds.Add(typedPayload.CreatedBy);
                }
                    break;
            }
        }

        var threadTasks = threadIds.Select(e => _coreServiceGrpcClient.GetThreadAsync(e, cancellationToken).AsTask());
        var userTasks = userIds.Select(e => _userServiceGrpcClient.GetUserAsync(e, cancellationToken).AsTask());

        var threads =
            (await Task.WhenAll(threadTasks)).ToDictionary(e => ThreadId.From(e.ThreadId),
                e => ThreadTitle.From(e.Title));
        var users = (await Task.WhenAll(userTasks)).ToDictionary(e => UserId.From(e.UserId), e => e.Username);

        return new InternalUserNotificationsDto
        {
            Notifications = userNotification,
            Threads = threads,
            Users = users
        };
    }
}