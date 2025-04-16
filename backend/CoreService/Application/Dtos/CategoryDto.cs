using CoreService.Application.Interfaces;
using CoreService.Domain.Abstractions;
using CoreService.Domain.ValueObjects;
using SharedKernel.Domain.ValueObjects;

namespace CoreService.Application.Dtos;

public sealed class CategoryDto : IHasForumId, IHasCreatedProperties
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public CategoryId CategoryId { get; set; }

    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Название категории
    /// </summary>
    public CategoryTitle Title { get; set; }

    /// <summary>
    /// Дата и время создания категории
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего категорию
    /// </summary>
    public UserId CreatedBy { get; set; }
}