using CoreService.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Domain.Entities;

/// <summary>
/// Разрешение
/// </summary>
[Include(typeof(User), PropertyGenerationMode.AsPrivateSet, nameof(User.UserId))]
public sealed partial class Grant : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор пользователя, создавшего разрешение
    /// </summary>
    public UserId CreatedBy { get; private set; }

    /// <summary>
    /// Идентификатор политики
    /// </summary>
    public PolicyId PolicyId { get; private set; }

    /// <summary>
    /// Дата и время создания разрешения
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    public Grant(UserId userId, PolicyId policyId, UserId createdBy, DateTime createdAt)
    {
        UserId = userId;
        PolicyId = policyId;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
    }
}