using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using ThreadState = CoreService.Domain.Enums.ThreadState;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>;

public sealed class GetCategoriesThreadsCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public required ThreadState? State { get; init; }

    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetCategoriesThreadsCountQueryHandler : IQueryHandler<GetCategoriesThreadsCountQuery,
    QueryResult>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesThreadsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryResult> HandleAsync(GetCategoriesThreadsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesThreadsCountAsync(query, cancellationToken);
    }
}