using CoreService.Application.Interfaces;
using CoreService.Domain.Entities;
using CoreService.Domain.Errors;
using OneOf;
using Shared.Application.Interfaces;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

[Include(typeof(Category), PropertyGenerationMode.AsRequired, nameof(Category.CategoryId))]
public sealed partial class GetCategoryQuery<T> : IQuery<OneOf<
    T,
    ForumAccessLevelError,
    CategoryAccessLevelError,
    ForumAccessRestrictedError,
    CategoryAccessRestrictedError,
    CategoryNotFoundError
>>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetCategoryQueryHandler<T> : IQueryHandler<GetCategoryQuery<T>, OneOf<
    T,
    ForumAccessLevelError,
    CategoryAccessLevelError,
    ForumAccessRestrictedError,
    CategoryAccessRestrictedError,
    CategoryNotFoundError
>>
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

    public async Task<OneOf<
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

        if (!accessCheckResult.TryPickT0(out _, out var accessErrors))
            return accessErrors.Match<OneOf<
                T,
                ForumAccessLevelError,
                CategoryAccessLevelError,
                ForumAccessRestrictedError,
                CategoryAccessRestrictedError,
                CategoryNotFoundError
            >>(
                e => e,
                e => e,
                e => e,
                e => e
            );

        var categoryResult = await _categoryReadRepository.GetOneAsync<T>(query.CategoryId, cancellationToken);

        return categoryResult.Match<OneOf<
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