using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetForumsCategoriesCountQuery
{
    public required IdList<ForumId> ForumIds { get; init; }
}

public sealed class GetForumsCategoriesCountQueryValidator : AbstractValidator<GetForumsCategoriesCountQuery>
{
    public GetForumsCategoriesCountQueryValidator()
    {
        RuleFor(e => e.ForumIds)
            .NotEmpty();
    }
}

public sealed class GetForumsCategoriesCountQueryHandler
{
    private readonly IForumReadRepository _repository;

    public GetForumsCategoriesCountQueryHandler(IForumReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<ForumId, long>> HandleAsync(GetForumsCategoriesCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetForumsCategoriesCountAsync(request, cancellationToken);
    }
}