using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsQuery : PagedQuery
{
    public enum GetCategoryThreadsQuerySortType
    {
        Activity
    }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<GetCategoryThreadsQuerySortType>? Sort { get; init; }
    
    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
}

public sealed class GetCategoryThreadsRequestValidator : PagedQueryValidator<GetCategoryThreadsQuery>
{
}

public sealed class GetCategoryThreadsQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoryThreadsQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(
        GetCategoryThreadsQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoryThreadsAsync<T>(request, cancellationToken);
    }

    public Task<IReadOnlyList<ThreadDto>> HandleAsync(
        GetCategoryThreadsQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<ThreadDto>(request, cancellationToken);
    }
}