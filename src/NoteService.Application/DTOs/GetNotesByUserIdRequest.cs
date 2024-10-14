using Common;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace NoteService.Application.DTOs;

public sealed class GetNotesByUserIdRequest : LongKeysetPageRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [FromRoute(Name = "userId")]
    public Guid UserId { get; set; }
}

public sealed class GetNotesByUserIdRequestValidator : LongKeysetPageRequestValidator<GetNotesByUserIdRequest>
{
    public GetNotesByUserIdRequestValidator()
    {
        RuleFor(e => e.UserId)
            .NotEmpty();
    }
}