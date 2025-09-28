using CoreService.Domain.ValueObjects;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Domain.Abstractions;
using UserService.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadSubscriptionWriteRepository
{
    public Task AddAsync(ThreadSubscription threadSubscription, CancellationToken cancellationToken);
    public Task<Result<Success, ThreadSubscriptionNotFoundError>> ExecuteRemoveAsync(UserId userId, ThreadId threadId, CancellationToken cancellationToken);
}