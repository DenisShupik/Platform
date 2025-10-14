using CoreService.Domain.Enums;
using CoreService.Domain.ValueObjects;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Тема
/// </summary>
public sealed class Thread
{
    /// <summary>
    /// Идентификатор темы
    /// </summary>
    public ThreadId ThreadId { get; private set; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; private set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public ThreadTitle Title { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего тему
    /// </summary>
    public UserId? CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания темы
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Состояние темы
    /// </summary>
    public ThreadStatus Status { get; private set; }

    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public PolicyId ReadPolicyId { get; private set; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public PolicyId PostCreatePolicyId { get; private set; }

    /// <summary>
    /// Сообщения темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }

    internal Thread(CategoryId categoryId, ThreadTitle title, UserId? createdBy, DateTime createdAt,
        PolicyId readPolicyId, PolicyId postCreatePolicyId)
    {
        ThreadId = ThreadId.From(Guid.CreateVersion7());
        CategoryId = categoryId;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        Status = ThreadStatus.Draft;
        ReadPolicyId = readPolicyId;
        PostCreatePolicyId = postCreatePolicyId;
    }
}