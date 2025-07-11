namespace CoreService.Application.Enums;

public enum ForumContainsFilter : byte
{
    /// <summary>
    /// Форум содержит разделы
    /// </summary>
    Category = 0,

    /// <summary>
    /// Форум содержит темы
    /// </summary>
    Thread = 1,

    /// <summary>
    /// Форум содержит сообщения
    /// </summary>
    Post = 2
}