using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesLatestPostQuery
{
    public required GuidIdList<CategoryId> CategoryIds { get; init; }
}

public sealed class GetCategoriesLatestPostQueryValidator : AbstractValidator<GetCategoriesLatestPostQuery>
{
    public GetCategoriesLatestPostQueryValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}

public sealed class GetCategoriesLatestPostQueryHandler
{
    private readonly IPostReadRepository _repository;

    public GetCategoriesLatestPostQueryHandler(IPostReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<CategoryId,T>> HandleAsync<T>(
        GetCategoriesLatestPostQuery request, CancellationToken cancellationToken
    )
    {
        return _repository.GetCategoriesLatestPostAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<CategoryId,PostDto>> HandleAsync(
        GetCategoriesLatestPostQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<PostDto>(request, cancellationToken);
    }
}