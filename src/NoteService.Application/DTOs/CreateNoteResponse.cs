namespace NoteService.Application.DTOs;

public sealed class CreateNoteResponse
{
    /// <summary>
    /// Идентификатор заметки
    /// </summary>
    public long NoteId { get; set; }
}