namespace NotificationService.Domain.Enums;

/// <summary>
/// Типы каналов доставки уведомлений
/// </summary>
public enum ChannelType : byte
{
    /// <summary>
    /// Внутренний канал
    /// </summary>
    Internal = 0,

    /// <summary>
    /// Электронная почта
    /// </summary>
    Email = 1
}