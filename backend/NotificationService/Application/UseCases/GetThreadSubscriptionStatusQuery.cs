using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class GetThreadSubscriptionStatusQuery;

public sealed class GetThreadSubscriptionStatusQueryResult
{
    /// <summary>
    /// Подписан ли пользователь на тему
    /// </summary>
    public required bool IsSubscribed { get; init; }
}

public sealed class GetThreadSubscriptionStatusQueryHandler
{
    private readonly IThreadSubscriptionReadRepository _threadSubscriptionReadRepository;

    public GetThreadSubscriptionStatusQueryHandler(
        IThreadSubscriptionReadRepository threadSubscriptionReadRepository
    )
    {
        _threadSubscriptionReadRepository = threadSubscriptionReadRepository;
    }

    public async Task<GetThreadSubscriptionStatusQueryResult> HandleAsync(GetThreadSubscriptionStatusQuery request,
        CancellationToken cancellationToken)
    {
        return new GetThreadSubscriptionStatusQueryResult
        {
            IsSubscribed =
                await _threadSubscriptionReadRepository.ExistsAsync(request.UserId, request.ThreadId,
                    cancellationToken)
        };
    }
}