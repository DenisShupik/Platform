using CoreService.Domain.Events;
using Hangfire;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Jobs;

namespace NotificationService.Infrastructure.Consumers;

public sealed class PostAddedEventConsumer
{
    private readonly IServiceProvider _serviceProvider;

    public PostAddedEventConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask ConsumeAsync(PostAddedEvent @event, CancellationToken cancellationToken)
    {
        var threadSubscriptionReadRepository = _serviceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();
        if (!await threadSubscriptionReadRepository.ExistsExcludingUserAsync(@event.ThreadId, @event.CreatedBy,
                cancellationToken)) return;
        var backgroundJobClient = _serviceProvider.GetRequiredService<IBackgroundJobClient>();
        backgroundJobClient.Enqueue<NotificationJob>(job =>
            job.ExecuteAsync(@event.ThreadId.Value, @event.PostId.Value, @event.CreatedBy.Value));
    }
}