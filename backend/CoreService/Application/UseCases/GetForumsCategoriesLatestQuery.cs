using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesLatestQuery
{
    public required IdList<ForumId> ForumIds { get; init; }
    public required int Count { get; init; }
}

public sealed class GetForumsCategoriesLatestQueryValidator : AbstractValidator<GetForumsCategoriesCountQuery>
{
    public GetForumsCategoriesLatestQueryValidator()
    {
        RuleFor(e => e.ForumIds)
            .NotEmpty();
    }
}

public sealed class GetForumsCategoriesLatestQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesLatestQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    private Task<Dictionary<ForumId, T[]>> HandleAsync<T>(
        GetForumsCategoriesLatestQuery request, CancellationToken cancellationToken
    ) where T : IHasForumId
    {
        return _repository.GetForumsCategoriesLatestAsync<T>(request, cancellationToken);
    }

    public Task<Dictionary<ForumId, CategoryDto[]>> HandleAsync(
        GetForumsCategoriesLatestQuery request, CancellationToken cancellationToken
    )
    {
        return HandleAsync<CategoryDto>(request, cancellationToken);
    }
}