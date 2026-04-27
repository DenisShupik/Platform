using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class GetCategoryQuery<T> : IQuery<Result<T, CategoryNotFoundError>>
    where T : notnull
{
    public required UserIdRole? QueriedBy { get; init; }
}

public sealed class
    GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>,
    Result<T, CategoryNotFoundError>>
    where T : notnull
{
    private readonly ICategoryReadRepository _categoryReadRepository;

    public GetCategoryQueryHandler(ICategoryReadRepository categoryReadRepository)
    {
        _categoryReadRepository = categoryReadRepository;
    }

    public Task<Result<T, CategoryNotFoundError>> HandleAsync(GetCategoryQuery<T> query,
        CancellationToken cancellationToken)
    {
        return _categoryReadRepository.GetOneAsync(query, cancellationToken);
    }
}