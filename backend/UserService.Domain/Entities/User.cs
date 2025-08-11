using UserService.Domain.ValueObjects;

namespace UserService.Domain.Entities;

public sealed class User
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    public Username Username { get; private set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Активна ли учетная запись пользователя
    /// </summary>
    public bool Enabled { get; private set; }

    /// <summary>
    /// Дата и время создания учетной записи пользователя
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    public User(UserId userId, Username username, string email, bool enabled, DateTime createdAt)
    {
        UserId = userId;
        Username = username;
        Email = email;
        Enabled = enabled;
        CreatedAt = createdAt;
    }
}