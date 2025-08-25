using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.Interfaces;
using SharedKernel.Application.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsQuery : IHasPagination<PaginationLimitMin10Max100Default100>
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
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }

    public required PaginationOffset? Offset { get; init; }
    public required PaginationLimitMin10Max100Default100? Limit { get;init; }
    public required SortCriteria<GetCategoryThreadsQuerySortType>? Sort { get; init; }
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