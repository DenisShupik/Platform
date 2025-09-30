using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.UseCases;

public enum GetCategoryThreadsPagedQuerySortType : byte
{
    Activity = 0
}

public sealed class
    GetCategoryThreadsPagedQuery<T> : SingleSortPagedQuery<Result<IReadOnlyList<T>, CategoryNotFoundError>,
    GetCategoryThreadsPagedQuerySortType>
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
}

public sealed class
    GetCategoryThreadsPagedQueryHandler<T> : IQueryHandler<GetCategoryThreadsPagedQuery<T>,
    Result<IReadOnlyList<T>, CategoryNotFoundError>>
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryThreadsPagedQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public Task<Result<IReadOnlyList<T>, CategoryNotFoundError>> HandleAsync(GetCategoryThreadsPagedQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _repository.GetCategoryThreadsAsync(query, cancellationToken);
    }
}