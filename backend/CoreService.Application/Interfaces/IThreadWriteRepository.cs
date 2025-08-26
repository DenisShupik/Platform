using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using OneOf;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.Interfaces;

public interface IThreadWriteRepository
{ 
    Task<OneOf<Thread, ThreadNotFoundError>> GetOneAsync(ThreadId threadId, CancellationToken cancellationToken);
    Task<OneOf<ThreadPostAddable, ThreadNotFoundError>> GetThreadPostAddableAsync(ThreadId threadId, CancellationToken cancellationToken);
}