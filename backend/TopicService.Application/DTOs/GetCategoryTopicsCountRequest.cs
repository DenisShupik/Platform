using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetCategoryTopicsCountRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
}

public sealed class GetCategoryTopicsCountRequestValidator : AbstractValidator<GetCategoryTopicsCountRequest>
{
    public GetCategoryTopicsCountRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}