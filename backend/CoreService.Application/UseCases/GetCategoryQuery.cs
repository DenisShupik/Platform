using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class GetCategoryQuery<T> : IQuery<Result<
    T,
    ForumAccessPolicyViolationError,
    CategoryAccessPolicyViolationError,
    ForumPolicyRestrictedError,
    CategoryPolicyRestrictedError,
    CategoryNotFoundError
>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>, Result<
    T,
    ForumAccessPolicyViolationError,
    CategoryAccessPolicyViolationError,
    ForumPolicyRestrictedError,
    CategoryPolicyRestrictedError,
    CategoryNotFoundError
>>
    where T : notnull
{
    private readonly IAccessRestrictionReadRepository _accessRestrictionReadRepository;
    private readonly ICategoryReadRepository _categoryReadRepository;

    public GetCategoryQueryHandler(
        IAccessRestrictionReadRepository accessRestrictionReadRepository,
        ICategoryReadRepository categoryReadRepository
    )
    {
        _categoryReadRepository = categoryReadRepository;
        _accessRestrictionReadRepository = accessRestrictionReadRepository;
    }

    public async Task<Result<
        T,
        ForumAccessPolicyViolationError,
        CategoryAccessPolicyViolationError,
        ForumPolicyRestrictedError,
        CategoryPolicyRestrictedError,
        CategoryNotFoundError
    >> HandleAsync(
        GetCategoryQuery<T> query, CancellationToken cancellationToken
    )
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy, query.CategoryId,
                cancellationToken);

        if (!accessCheckResult.TryGetOrExtend<T, CategoryNotFoundError>(out _, out var accessErrors))
            return accessErrors.Value;

        var categoryResult = await _categoryReadRepository.GetOneAsync<T>(query.CategoryId, cancellationToken);

        return categoryResult;
    }
}