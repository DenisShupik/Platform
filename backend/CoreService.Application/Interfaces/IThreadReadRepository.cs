using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IThreadReadRepository
{
    public Task<Result<T, ThreadNotFoundError, PolicyViolationError, ReadPolicyRestrictedError>> GetOneAsync<T>(
        GetThreadQuery<T> query, CancellationToken cancellationToken)
        where T : notnull;

    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ThreadId, Guid> ids, CancellationToken cancellationToken);
    public Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> request, CancellationToken cancellationToken);
    public Task<ulong> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, ulong>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(GetThreadsPostsLatestQuery<T> query,
        CancellationToken cancellationToken) where T : IHasThreadId;
}