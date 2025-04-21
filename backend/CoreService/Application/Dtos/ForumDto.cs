using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class ForumDto : IHasForumId, IHasCreateProperties
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Название форума
    /// </summary>
    public ForumTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания форума
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего форум
    /// </summary>
    public UserId CreatedBy { get; set; }
}