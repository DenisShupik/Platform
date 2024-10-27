using FluentValidation;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreateSectionRequest
{
    /// <summary>
    /// Наименование раздела
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateSectionRequestValidator : AbstractValidator<CreateSectionRequest>
{
    public CreateSectionRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(Section.TitleMaxLength);
    }
}