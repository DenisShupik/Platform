using System.Data;
using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Interfaces;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Models;

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

    [TickerFunction(nameof(NotificationJob))]
    public async Task ExecuteAsync(TickerFunctionContext<PostAddedEvent> tickerContext, CancellationToken cancellation)
    {
        try
        {
            var request = tickerContext.Request;
            
            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellation);

            var notificationData = new PostAddedNotificationData
            {
                ThreadId = request.ThreadId,
                PostId = request.PostId,
            };
            var notification = new Notification(notificationData);

            await _notificationRepository.AddAsync(notification, cancellation);
            await _unitOfWork.SaveChangesAsync(cancellation);

            await _notificationDeliveryRepository.BulkAddThreadNotificationDeliveryAsync(notification.NotificationId,
                request.ThreadId, request.CreatedBy, cancellation);

            await transaction.CommitAsync(cancellation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке уведомления для поста {PostId}", tickerContext.Request.PostId);
            throw;
        }
    }
}