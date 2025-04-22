using CoreService.Application.UseCases;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using OneOf;

namespace CoreService.Application.Interfaces;

public interface IThreadReadRepository
{
    public Task<OneOf<T, ThreadNotFoundError>> GetOneAsync<T>(ThreadId id, CancellationToken cancellationToken);
    public Task<IReadOnlyList<T>> GetBulkAsync<T>(List<ThreadId> ids, CancellationToken cancellationToken);
    public Task<List<T>> GetAllAsync<T>(GetThreadsQuery request, CancellationToken cancellationToken);
    public Task<long> GetCountAsync(GetThreadsCountQuery request, CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, long>> GetThreadsPostsCountAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken);

    public Task<Dictionary<ThreadId, T>> GetThreadsPostsLatestAsync<T>(GetThreadsPostsLatestQuery request,
        CancellationToken cancellationToken) where T : IHasThreadId;

    public Task<OneOf<long, PostNotFoundError>> GetPostOrderAsync(ThreadId threadId, PostId postId,
        CancellationToken cancellationToken);
}