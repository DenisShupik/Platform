namespace CoreService.Domain.Enums;

/// <summary>
/// Состояние темы
/// </summary>
public enum ThreadState : byte
{
    /// <summary>
    /// Тема еще подготавливается автором
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Тема ожидает подтверждения
    /// </summary>
    PendingApproval = 1,

    /// <summary>
    /// Тема опубликована и доступна пользователям
    /// </summary>
    Approved = 2
}