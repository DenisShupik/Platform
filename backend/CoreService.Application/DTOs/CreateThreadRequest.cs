using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using CoreService.Domain.Entities;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.DTOs;

public sealed class CreateThreadRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public long CategoryId { get; set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateThreadRequestValidator : AbstractValidator<CreateThreadRequest>
{
    public CreateThreadRequestValidator()
    {
        RuleFor(e => e.CategoryId)
            .GreaterThan(0);

        RuleFor(e => e.Title)
            .MaximumLength(Thread.TitleMaxLength);
    }
}