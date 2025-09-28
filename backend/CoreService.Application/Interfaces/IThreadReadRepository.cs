using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions;

namespace CoreService.Application.Interfaces;

public interface IThreadReadRepository
{
    public Task<Result<T, ThreadNotFoundError>> GetOneAsync<T>(ThreadId id, CancellationToken cancellationToken) where T : notnull;
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(IdSet<ThreadId,Guid> ids, CancellationToken cancellationToken);
    public Task<List<T>> GetAllAsync<T>(GetThreadsPagedQuery<T> request, CancellationToken cancellationToken);
    public Task<ulong> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, ulong>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(GetThreadsPostsLatestQuery<T> request,
        CancellationToken cancellationToken) where T : IHasThreadId;

    public Task<Result<PostIndex, PostNotFoundError>> GetPostIndexAsync(PostId postId,
        CancellationToken cancellationToken);
}