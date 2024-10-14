using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace NoteService.Application.DTOs;

public sealed class GetNotesByUserIdCountRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [FromRoute(Name = "userId")]
    public Guid UserId { get; set; }
}

public sealed class GetNotesByUserIdCountRequestValidator : AbstractValidator<GetNotesByUserIdRequest>
{
    public GetNotesByUserIdCountRequestValidator()
    {
        RuleFor(e => e.UserId)
            .NotEmpty();
    }
}