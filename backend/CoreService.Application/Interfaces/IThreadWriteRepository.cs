using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Enums;
using Shared.Domain.Abstractions.Results;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Interfaces;

public interface IThreadWriteRepository
{
    Task<Result<Thread, ThreadNotFoundError>> GetOneAsync(ThreadId threadId, LockMode lockMode, CancellationToken cancellationToken);
    public void Add(Thread thread);
}