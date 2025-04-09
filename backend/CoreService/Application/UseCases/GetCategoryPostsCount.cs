using CoreService.Domain.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Application.Abstractions;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public GuidIdList<CategoryId> CategoryIds { get; set; }
}

public sealed class GetCategoryPostsCountRequestValidator : AbstractValidator<GetCategoryPostsCountRequest>
{
    public GetCategoryPostsCountRequestValidator()
    {
        RuleFor(e => e.CategoryIds)
            .NotEmpty();
    }
}