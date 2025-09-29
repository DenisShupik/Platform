namespace CoreService.Domain.Enums;

/// <summary>
/// Уровень доступа
/// </summary>
public enum AccessLevel : byte
{
    /// <summary>
    /// Доступ разрешен всем пользователям, включая неавторизованных (анонимных) посетителей
    /// </summary>
    Public = 0,

    /// <summary>
    /// Доступ разрешен только авторизованным пользователям
    /// </summary>
    Authenticated = 1,

    /// <summary>
    /// Доступ разрешен только определённым пользователям
    /// </summary>
    Restricted = 2
}