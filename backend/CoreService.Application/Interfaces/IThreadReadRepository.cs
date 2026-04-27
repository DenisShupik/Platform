using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.Interfaces;

public interface IThreadReadRepository
{
    public Task<Result<T, ThreadNotFoundError, PermissionDeniedError>> GetOneAsync<T>(GetThreadQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    public Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError>>> GetBulkAsync<T>(
        GetThreadsBulkQuery<T> query,
        CancellationToken cancellationToken) where T : notnull;

    public Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> request, CancellationToken cancellationToken);
    public Task<Count> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>>
        GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
            CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
        GetThreadsPostsLatestAsync<T>(GetThreadsPostsLatestQuery<T> query,
            CancellationToken cancellationToken) where T : IHasThreadId;
}