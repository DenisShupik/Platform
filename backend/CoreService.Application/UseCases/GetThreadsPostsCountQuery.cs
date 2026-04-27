using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<ThreadId, Result<Count, ThreadNotFoundError, PermissionDeniedError>>;

public sealed class GetThreadsPostsCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Идентификаторы тем
    /// </summary>
    public required IdSet<ThreadId, Guid> ThreadIds { get; init; }
    
    /// <summary>
    /// Статус темы
    /// </summary>
    public required ThreadState? Status { get; init; }
    
    public required UserIdRole? QueriedBy {get; init; }
}

public sealed class
    GetThreadsPostsCountQueryHandler : IQueryHandler<GetThreadsPostsCountQuery, QueryResult>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsPostsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryResult> HandleAsync(GetThreadsPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetThreadsPostsCountAsync(request, cancellationToken);
    }
}