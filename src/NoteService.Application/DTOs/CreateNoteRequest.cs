using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace NoteService.Application.DTOs;

public sealed class CreateNoteRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [FromRoute(Name = "userId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Название заметки
    /// </summary>
    [FromBody]
    public string Title { get; set; } = null!;
}

public sealed class CreateNoteRequestRequestValidator : AbstractValidator<CreateNoteRequest>
{
    public CreateNoteRequestRequestValidator()
    {
        RuleFor(e => e.UserId)
            .NotEmpty();

        RuleFor(e => e.Title)
            .MaximumLength(256);
    }
}