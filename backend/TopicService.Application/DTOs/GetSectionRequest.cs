using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TopicService.Application.DTOs;

public sealed class GetSectionRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [FromRoute]
    public long SectionId { get; set; }
}

public sealed class GetSectionRequestValidator : AbstractValidator<GetSectionRequest>
{
    public GetSectionRequestValidator()
    {
        RuleFor(e => e.SectionId)
            .GreaterThan(0);
    }
}