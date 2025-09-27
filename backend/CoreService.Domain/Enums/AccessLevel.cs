namespace CoreService.Domain.Enums;

/// <summary>
/// Уровень доступа
/// </summary>
public enum AccessLevel : byte
{
    /// <summary>
    /// Доступен всем пользователям, включая неавторизованных (анонимных) посетителей
    /// </summary>
    Public = 0,

    /// <summary>
    /// Доступен только авторизованным пользователям
    /// </summary>
    Authenticated = 1
}