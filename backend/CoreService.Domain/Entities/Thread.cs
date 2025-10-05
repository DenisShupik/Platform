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
    /// Идентификатор набора политик темы
    /// </summary>
    public ThreadPolicySetId? ThreadPolicySetId { get; private set; }

    /// <summary>
    /// Сообщения темы
    /// </summary>
    public ICollection<Post> Posts { get; set; }

    private Thread()
    {
    }

    internal Thread(CategoryId categoryId, ThreadTitle title, UserId? createdBy, DateTime createdAt,
        ThreadPolicySetId? threadPolicySetId)
    {
        ThreadId = ThreadId.From(Guid.CreateVersion7());
        CategoryId = categoryId;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        Status = ThreadStatus.Draft;
        ThreadPolicySetId = threadPolicySetId;
    }
}