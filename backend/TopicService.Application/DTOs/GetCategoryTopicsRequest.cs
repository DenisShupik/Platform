using SharedKernel;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetCategoryTopicsRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    [FromRoute]
    public long CategoryId { get; set; }
}

public sealed class GetCategoryTopicsRequestValidator : LongKeysetPageRequestValidator<GetCategoryTopicsRequest>
{
    public GetCategoryTopicsRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);
    }
}