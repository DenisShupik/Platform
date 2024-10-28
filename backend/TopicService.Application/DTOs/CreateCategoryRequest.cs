using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreateCategoryRequest
{
    public sealed class FromBody
    {
        /// <summary>
        /// Наименование категории
        /// </summary>
        public string Title { get; set; }
    }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    [FromRoute]
    public long SectionId { get; set; }

    [FromBody] public FromBody Body { get; set; }
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(e => e.SectionId)
            .GreaterThan(0);

        RuleFor(e => e.Body.Title)
            .MaximumLength(Category.TitleMaxLength);
    }
}