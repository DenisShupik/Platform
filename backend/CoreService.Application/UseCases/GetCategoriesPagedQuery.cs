using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;
using SharedKernel.Application.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPagedQuery : PagedQuery<PaginationLimitMin10Max100Default100,
    GetCategoriesPagedQuery.GetCategoriesPagedQuerySortType>
{
    public enum GetCategoriesPagedQuerySortType : byte
    {
        CategoryId = 0,
        ForumId = 1
    }

    /// <summary>
    /// Идентификаторы форумов
    /// </summary>
    public required IdSet<ForumId>? ForumIds { get; init; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public required CategoryTitle? Title { get; init; }
}

public sealed class GetCategoriesPagedQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPagedQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetCategoriesPagedQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<GetCategoriesPagedQueryResult> HandleAsync(GetCategoriesPagedQuery request,
        CancellationToken cancellationToken)
    {
        return await HandleAsync<CategoryDto>(request, cancellationToken);
    }
}