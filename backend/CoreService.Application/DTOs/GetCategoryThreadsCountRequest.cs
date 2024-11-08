using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryThreadsCountRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
}

public sealed class GetCategoryThreadsCountRequestValidator : AbstractValidator<GetCategoryThreadsCountRequest>
{
    public GetCategoryThreadsCountRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}