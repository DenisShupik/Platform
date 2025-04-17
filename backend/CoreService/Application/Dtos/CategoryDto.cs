using CoreService.Application.Interfaces;
using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class CategoryDto : IHasForumId, IHasCreatedProperties
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
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public UserId CreatedBy { get; set; }
}