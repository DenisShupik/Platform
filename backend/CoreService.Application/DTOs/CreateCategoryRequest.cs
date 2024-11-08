using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using CoreService.Domain.Entities;

namespace CoreService.Application.DTOs;

public sealed class CreateCategoryRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public long ForumId { get; set; }

    /// <summary>
    /// Наименование категории
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(e => e.ForumId)
            .GreaterThan(0);

        RuleFor(e => e.Title)
            .MaximumLength(Category.TitleMaxLength);
    }
}