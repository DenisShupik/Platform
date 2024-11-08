using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CoreService.Application.DTOs;

public sealed class GetCategoryRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
}

public sealed class GetCategoryRequestValidator : AbstractValidator<GetCategoryRequest>
{
    public GetCategoryRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}