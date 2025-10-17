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
[Include(typeof(Policy), PropertyGenerationMode.AsPrivateSet, nameof(Policy.PolicyId))]
public sealed partial class Grant : IHasCreateProperties
{
    /// <summary>
    /// Идентификатор пользователя, создавшего разрешение
    /// </summary>
    public UserId CreatedBy { get; private set; }

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