using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using SharedKernel.Sorting;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsQuery : PaginatedQuery
{
    public enum GetCategoryThreadsRequestSortType
    {
        Activity
    }

    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Сортировка
    /// </summary>
    public required SortCriteria<GetCategoryThreadsRequestSortType>? Sort { get; init; }
}

public sealed class GetCategoryThreadsRequestValidator : PaginatedQueryValidator<GetCategoryThreadsQuery>
{
}

public sealed class GetCategoryThreadsQueryHandler
{
    private readonly IThreadReadRepository _repository;

    public GetCategoryThreadsQueryHandler(IThreadReadRepository repository)
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