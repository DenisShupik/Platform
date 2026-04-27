using CoreService.Application.Interfaces;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Application.UseCases;

using QueryResult = Count;

public sealed class GetThreadsCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required UserId? CreatedBy { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public required ThreadState? State { get; init; }

    /// <summary>
    /// Идентификатор пользователя, запросившего данные
    /// </summary>
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class GetThreadsCountQueryHandler : IQueryHandler<GetThreadsCountQuery, QueryResult>
{
    private readonly IThreadReadRepository _repository;

    public GetThreadsCountQueryHandler(IThreadReadRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryResult> HandleAsync(GetThreadsCountQuery query, CancellationToken cancellationToken)
    {
        return _repository.GetCountAsync(query, cancellationToken);
    }
}