using CoreService.Domain.Events;
using NotificationService.Application.Interfaces;
using NotificationService.Infrastructure.Jobs;
using TickerQ.Utilities;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models.Ticker;

namespace NotificationService.Infrastructure.Consumers;

public sealed class PostAddedEventConsumer(IServiceProvider serviceProvider)
{
    public async ValueTask ConsumeAsync(PostAddedEvent @event, CancellationToken cancellationToken)
    {
        var threadSubscriptionReadRepository = serviceProvider.GetRequiredService<IThreadSubscriptionReadRepository>();
        if (!await threadSubscriptionReadRepository.ExistsExcludingUserAsync(@event.ThreadId, @event.CreatedBy,
                cancellationToken)) return;
        
        var timeTickerManager = serviceProvider.GetRequiredService<ITimeTickerManager<TimeTicker>>();
        await timeTickerManager.AddAsync(new TimeTicker
        {
            Request = TickerHelper.CreateTickerRequest(@event),
            ExecutionTime = DateTime.UtcNow,
            Function = nameof(NotificationJob),
            Retries = 3,
            RetryIntervals = [20, 60, 100]
        }, cancellationToken);
    }
}