using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesPostsLatestQuery
{
    public required GuidIdList<CategoryId> CategoryIds { get; init; }
}

public sealed class GetCategoriesPostsLatestQueryValidator : AbstractValidator<GetCategoriesPostsLatestQuery>
{
    public GetCategoriesPostsLatestQueryValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}

public sealed class GetCategoriesPostsLatestQueryHandler
{
    private readonly IPostReadRepository _repository;

    public GetCategoriesPostsLatestQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<CategoryId,T>> HandleAsync<T>(
        GetCategoriesPostsLatestQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesPostsLatestAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<CategoryId,PostDto>> HandleAsync(
        GetCategoriesPostsLatestQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<PostDto>(request, cancellationToken);
    }
}