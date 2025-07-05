using Generator.Attributes;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using SharedKernel.Application.Interfaces;

namespace NotificationService.Application.UseCases;

[IncludeAsRequired(typeof(ThreadSubscription), nameof(ThreadSubscription.UserId), nameof(ThreadSubscription.ThreadId))]
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
    private readonly IUnitOfWork _unitOfWork;

    public GetThreadSubscriptionStatusQueryHandler(
        IThreadSubscriptionReadRepository threadSubscriptionReadRepository,
        IUnitOfWork unitOfWork
    )
    {
        _threadSubscriptionReadRepository = threadSubscriptionReadRepository;
        _unitOfWork = unitOfWork;
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