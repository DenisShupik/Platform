using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<CategoryId, Result<Count, CategoryNotFoundError>>;

public sealed class GetCategoriesPostsCountQuery : IQuery<QueryResult>
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdSet<CategoryId, Guid> CategoryIds { get; init; }

    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetCategoriesPostsCountQueryHandler : IQueryHandler<GetCategoriesPostsCountQuery, QueryResult>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<QueryResult> HandleAsync(GetCategoriesPostsCountQuery query,
        CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesPostsCountAsync(query, cancellationToken);
    }
}