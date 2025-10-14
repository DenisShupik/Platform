using CoreService.Domain.ValueObjects;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Форум
/// </summary>
public sealed class Forum
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; private set; }

    /// <summary>
    /// Название форума
    /// </summary>
    public ForumTitle Title { get; private set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public UserId? CreatedBy { get; private set; }

    /// <summary>
    /// Дата и время создания форума
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Идентификатор политики доступа
    /// </summary>
    public PolicyId ReadPolicyId { get; private set; }

    /// <summary>
    /// Идентификатор политики создания раздела
    /// </summary>
    public PolicyId CategoryCreatePolicyId { get; private set; }

    /// <summary>
    /// Идентификатор политики создания темы
    /// </summary>
    public PolicyId ThreadCreatePolicyId { get; private set; }

    /// <summary>
    /// Идентификатор политики создания сообщения
    /// </summary>
    public PolicyId PostCreatePolicyId { get; private set; }

    /// <summary>
    /// Разделы форума
    /// </summary>
    public ICollection<Category> Categories { get; set; }

    internal Forum(ForumTitle title, UserId? createdBy, DateTime createdAt, PolicyId readPolicyId,
        PolicyId categoryCreatePolicyId, PolicyId threadCreatePolicyId, PolicyId postCreatePolicyId)
    {
        ForumId = ForumId.From(Guid.CreateVersion7());
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        ReadPolicyId = readPolicyId;
        CategoryCreatePolicyId = categoryCreatePolicyId;
        ThreadCreatePolicyId = threadCreatePolicyId;
        PostCreatePolicyId = postCreatePolicyId;
    }
}