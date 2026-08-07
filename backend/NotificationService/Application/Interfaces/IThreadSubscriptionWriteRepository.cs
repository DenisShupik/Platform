using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionWriteRepository
{
    public void Add(ThreadSubscription threadSubscription);
    public Task<Result<Success, ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
}
