using CoreService.Application.Interfaces;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetThreadsPostsLatestQuery<T> : IQuery<Dictionary<ThreadId, T>> where T : IHasThreadId
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
}

public sealed class
    GetThreadsPostsLatestQueryHandler<T> : IQueryHandler<GetThreadsPostsLatestQuery<T>, Dictionary<ThreadId, T>>
    where T : IHasThreadId
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsLatestQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<ThreadId, T>> HandleAsync(
        GetThreadsPostsLatestQuery<T> query, CancellationToken cancellationToken
    )
    {
        return _repository.GetThreadsPostsLatestAsync(query, cancellationToken);
    }
}