using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TopicService.Domain.Entities;

namespace TopicService.Application.DTOs;

public sealed class CreateCategoryRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public long SectionId { get; set; }

    /// <summary>
    /// Наименование категории
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(e => e.SectionId)
            .GreaterThan(0);

        RuleFor(e => e.Title)
            .MaximumLength(Category.TitleMaxLength);
    }
}