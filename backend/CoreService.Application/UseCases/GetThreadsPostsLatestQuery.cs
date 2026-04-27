using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class
    GetThreadsPostsLatestQuery<T> : IQuery<
    Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
    where T : IHasThreadId
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
    
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetThreadsPostsLatestQueryHandler<T> : IQueryHandler<GetThreadsPostsLatestQuery<T>,
    Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
    where T : IHasThreadId
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsLatestQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Dictionary<ThreadId, Result<T, ThreadNotFoundError, PermissionDeniedError, PostNotFoundError>>>
        HandleAsync(GetThreadsPostsLatestQuery<T> query,
            CancellationToken cancellationToken)
    {
        return _repository.GetThreadsPostsLatestAsync(query, cancellationToken);
    }
}