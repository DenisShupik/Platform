using System.Data;
using CoreService.Domain.ValueObjects;
using Hangfire;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Jobs;

public sealed class NotificationJob
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationDeliveryRepository _notificationDeliveryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationJob> _logger;

    public NotificationJob(
        INotificationRepository notificationRepository,
        INotificationDeliveryRepository notificationDeliveryRepository,
        IUnitOfWork unitOfWork,
        ILogger<NotificationJob> logger
    )
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _notificationDeliveryRepository = notificationDeliveryRepository;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = [30, 60, 300])]
    public async Task ExecuteAsync(Guid threadId, long postId, Guid userId)
    {
        try
        {
            _logger.LogInformation("Обработка уведомления для поста {PostId} в треде {ThreadId}",
                postId, threadId);

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

            var notificationData = new PostAddedNotificationData
            {
                ThreadId = ThreadId.From(threadId),
                PostId = PostId.From(postId)
            };
            var notification = new Notification(notificationData);

            await _notificationRepository.AddAsync(notification, CancellationToken.None);
            await _unitOfWork.SaveChangesAsync(CancellationToken.None);

            await _notificationDeliveryRepository.BulkAddThreadNotificationDeliveryAsync(notification.NotificationId,
                ThreadId.From(threadId), UserId.From(userId), CancellationToken.None);

            await transaction.CommitAsync();

            _logger.LogInformation("Уведомления для поста {PostId} успешно обработаны", postId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке уведомления для поста {PostId}", postId);
            throw;
        }
    }
}