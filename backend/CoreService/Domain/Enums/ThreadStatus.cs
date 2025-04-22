namespace CoreService.Domain.Enums;

/// <summary>
/// Состояние темы
/// </summary>
public enum ThreadStatus : byte
{
    /// <summary>
    /// Тема еще подготавливается автором
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Тема опубликована и доступна пользователям
    /// </summary>
    Published = 1
}