namespace CoreService.Domain.Enums;

/// <summary>
/// Уровень ограничения
/// </summary>
public enum RestrictionLevel : byte
{
    /// <summary>
    /// Полный запрет
    /// </summary>
    Deny = 0,

    /// <summary>
    /// Только чтение
    /// </summary>
    ReadOnly = 1
}