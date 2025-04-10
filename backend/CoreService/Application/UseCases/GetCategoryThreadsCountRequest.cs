using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryThreadsCountRequest
{
    /// <summary>
    /// Идентификаторы категории
    /// </summary>
    [FromRoute]
    public GuidIdList<CategoryId> CategoryIds { get; set; }
}

public sealed class GetCategoryThreadsCountRequestValidator : AbstractValidator<GetCategoryThreadsCountRequest>
{
    public GetCategoryThreadsCountRequestValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}