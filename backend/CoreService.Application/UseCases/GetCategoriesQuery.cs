using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesQuery : PaginatedQuery
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public required ForumId? ForumId { get; init; }
    
    /// <summary>
    /// Название раздела
    /// </summary>
    public required CategoryTitle? Title {get; init; }
}

public sealed class GetCategoriesQueryValidator : PaginatedQueryValidator<GetCategoriesQuery>
{
}

public sealed class GetCategoriesQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    private Task<IReadOnlyList<T>> HandleAsync<T>(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return _repository.GetAllAsync<T>(request, cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryDto>> HandleAsync(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await HandleAsync<CategoryDto>(request, cancellationToken);
    }
}