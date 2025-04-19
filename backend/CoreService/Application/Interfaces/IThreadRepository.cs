using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IThreadRepository
{
    public Task<OneOf<T, ThreadNotFoundError>> GetWithLockAsync<T>(ThreadId threadId,
        CancellationToken cancellationToken) where T : class, IHasThreadId;
}