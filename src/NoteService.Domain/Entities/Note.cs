namespace NoteService.Domain.Entities;

public sealed class Note
{
    /// <summary>
    /// Идентификатор заметки
    /// </summary>
    public long NoteId { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// Название заметки
    /// </summary>
    public string Title { get; set; } = null!;
}