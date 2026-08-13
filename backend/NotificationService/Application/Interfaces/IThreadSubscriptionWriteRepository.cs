using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionWriteRepository
{
    public Task<SuccessOr<DuplicateThreadSubscriptionError>> ExecuteAddAsync(
        ThreadSubscription threadSubscription,
        CancellationToken cancellationToken);

    public Task<SuccessOr<ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
}
