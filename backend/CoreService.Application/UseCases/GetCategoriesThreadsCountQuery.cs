using CoreService.Application.Interfaces;
using CoreService.Domain.ValueObjects;
using FluentValidation;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoriesThreadsCountQuery
{
    /// <summary>
    /// Идентификаторы разделов
    /// </summary>
    public required IdList<CategoryId> CategoryIds { get; init; }
    
    /// <summary>
    /// Включать ли в отбор черновики тем
    /// </summary>
    public required bool IncludeDraft { get; init; }
}

public sealed class GetCategoriesThreadsCountQueryValidator : AbstractValidator<GetCategoriesThreadsCountQuery>
{
    public GetCategoriesThreadsCountQueryValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}

public sealed class GetCategoriesThreadsCountQueryHandler
{
    private readonly ICategoryReadRepository _repository;

    public GetCategoriesThreadsCountQueryHandler(ICategoryReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Dictionary<CategoryId, long>> HandleAsync(GetCategoriesThreadsCountQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _repository.GetCategoriesThreadsCountAsync(request, cancellationToken);
    }
}