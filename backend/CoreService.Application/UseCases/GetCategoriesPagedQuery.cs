using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPagedQuery : PagedQuery<GetCategoriesPagedQuery.SortType>
{
    public enum SortType : byte
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