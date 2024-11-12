using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.Batching;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryPostsCountRequest
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    [FromRoute]
    public LongIds CategoryIds { get; set; }
}

public sealed class GetCategoryPostsCountRequestValidator : AbstractValidator<GetCategoryPostsCountRequest>
{
    public GetCategoryPostsCountRequestValidator()
    {
        RuleForEach(e => e.CategoryIds)
            .GreaterThan(0);
    }
}