using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class GetCategoryQuery<T> : IQuery<Result<
    T,
    CategoryNotFoundError,
    PolicyViolationError,
    PolicyRestrictedError
>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>, Result<
    T,
    CategoryNotFoundError,
    PolicyViolationError,
    PolicyRestrictedError
>>
    where T : notnull
{
    private readonly ICategoryReadRepository _categoryReadRepository;

    public GetCategoryQueryHandler(
        ICategoryReadRepository categoryReadRepository
    )
    {
        _categoryReadRepository = categoryReadRepository;
    }

    public async Task<Result<
        T,
        CategoryNotFoundError,
        PolicyViolationError,
        PolicyRestrictedError
    >> HandleAsync(
        GetCategoryQuery<T> query, CancellationToken cancellationToken
    )
    {
       
        var categoryResult = await _categoryReadRepository.GetOneAsync(query, cancellationToken);

        return categoryResult;
    }
}