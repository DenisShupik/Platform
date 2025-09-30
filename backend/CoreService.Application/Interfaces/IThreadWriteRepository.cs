using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Interfaces;

public interface IThreadWriteRepository
{ 
    Task<Result<Thread, ThreadNotFoundError>> GetOneAsync(ThreadId threadId, CancellationToken cancellationToken);
    Task<Result<ThreadPostAddable, ThreadNotFoundError>> GetThreadPostAddableAsync(ThreadId threadId, CancellationToken cancellationToken);
}