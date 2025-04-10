using CoreService.Domain.ValueObjects;
using FluentValidation;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

public sealed class CreateThreadRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public CategoryId CategoryId { get; set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
{
    public CreateThreadRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(Thread.TitleMaxLength);
    }
}