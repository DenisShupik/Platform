using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.UserId),
    nameof(ThreadSubscription.ThreadId))]
public sealed partial class GetThreadSubscriptionStatusQuery : IQuery<GetThreadSubscriptionStatusQueryResult>;

public sealed class GetThreadSubscriptionStatusQueryResult
{
    /// <summary>
    /// Подписан ли пользователь на тему
    /// </summary>
    public required bool IsSubscribed { get; init; }
}

public sealed class
    GetThreadSubscriptionStatusQueryHandler : IQueryHandler<GetThreadSubscriptionStatusQuery,
    GetThreadSubscriptionStatusQueryResult>
{
    private readonly IThreadSubscriptionReadRepository _threadSubscriptionReadRepository;

    public GetThreadSubscriptionStatusQueryHandler(
        IThreadSubscriptionReadRepository threadSubscriptionReadRepository
    )
    {
        _threadSubscriptionReadRepository = threadSubscriptionReadRepository;
    }

    public async Task<GetThreadSubscriptionStatusQueryResult> HandleAsync(GetThreadSubscriptionStatusQuery query,
        CancellationToken cancellationToken)
    {
        return new GetThreadSubscriptionStatusQueryResult
        {
            IsSubscribed =
                await _threadSubscriptionReadRepository.ExistsAsync(query.UserId, query.ThreadId,
                    cancellationToken)
        };
    }
}