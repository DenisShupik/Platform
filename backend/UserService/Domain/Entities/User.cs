namespace UserService.Domain.Entities;

public sealed class User
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Активна ли учетная запись пользователя
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Дата и время создания учетной записи пользователя
    /// </summary>
    public DateTime CreatedAt { get; set; }
}