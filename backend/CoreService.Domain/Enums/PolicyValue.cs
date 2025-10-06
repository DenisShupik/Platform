namespace CoreService.Domain.Enums;

/// <summary>
/// Политика
/// </summary>
public enum PolicyValue : byte
{
    /// <summary>
    /// Любой пользователь, включая неавторизованных посетителей
    /// </summary>
    Any = 0,

    /// <summary>
    /// Авторизованный пользователь
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// Пользователь, которому предоставлено разрешение
    /// </summary>
    Granted = 2
}