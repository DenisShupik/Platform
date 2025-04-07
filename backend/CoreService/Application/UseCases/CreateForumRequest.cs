using CoreService.Domain.Entities;
using FluentValidation;

namespace CoreService.Application.UseCases;

public sealed class CreateForumRequest
{
    /// <summary>
    /// Наименование раздела
    /// </summary>
    public string Title { get; set; }
}

public sealed class CreateForumRequestValidator : AbstractValidator<CreateForumRequest>
{
    public CreateForumRequestValidator()
    {
        RuleFor(e => e.Title)
            .MaximumLength(Forum.TitleMaxLength);
    }
}