using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsCountQuery
{
    public required IdList<CategoryId> CategoryIds { get; init; }
}

public sealed class GetCategoriesPostsCountQueryValidator : AbstractValidator<GetCategoriesPostsCountQuery>
{
    public GetCategoriesPostsCountQueryValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}

public sealed class GetCategoriesPostsCountQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesPostsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<CategoryId, long>> HandleAsync(GetCategoriesPostsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetCategoriesPostsCountAsync(request, cancellationToken);
    }
}