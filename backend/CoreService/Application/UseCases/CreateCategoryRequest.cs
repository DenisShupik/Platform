using CoreService.Domain.Entities;
using CoreService.Domain.ValueObjects;
using FluentValidation;

namespace CoreService.Application.UseCases;

public sealed class CreateCategoryRequest
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Наименование категории
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(Category.TitleMaxLength);
    }
}