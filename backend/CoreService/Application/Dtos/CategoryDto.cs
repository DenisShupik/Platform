using CoreService.Domain.Interfaces;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class CategoryDto : IHasCategoryId, IHasForumId, IHasCreateProperties
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public CategoryId CategoryId { get; set; }

    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public CategoryTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public UserId CreatedBy { get; set; }
}