using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class GetCategoryQuery<T> : IQuery<Result<
    T,
    ForumAccessLevelError,
    CategoryAccessLevelError,
    ForumAccessRestrictedError,
    CategoryAccessRestrictedError,
    CategoryNotFoundError
>>
    where T : notnull
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>, Result<
    T,
    ForumAccessLevelError,
    CategoryAccessLevelError,
    ForumAccessRestrictedError,
    CategoryAccessRestrictedError,
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
        ForumAccessLevelError,
        CategoryAccessLevelError,
        ForumAccessRestrictedError,
        CategoryAccessRestrictedError,
        CategoryNotFoundError
    >> HandleAsync(
        GetCategoryQuery<T> query, CancellationToken cancellationToken
    )
    {
        var accessCheckResult =
            await _accessRestrictionReadRepository.CheckUserAccessAsync(query.QueriedBy, query.CategoryId,
                cancellationToken);

        if (!accessCheckResult.TryPickOrExtend<T, CategoryNotFoundError>(out _, out var accessErrors))
            return accessErrors.Value;

        var categoryResult = await _categoryReadRepository.GetOneAsync<T>(query.CategoryId, cancellationToken);

        return categoryResult.Match<Result<
            T,
            ForumAccessLevelError,
            CategoryAccessLevelError,
            ForumAccessRestrictedError,
            CategoryAccessRestrictedError,
            CategoryNotFoundError
        >>(
            category => category,
            notfound => notfound
        );
    }
}