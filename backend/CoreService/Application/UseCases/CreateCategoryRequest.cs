using CoreService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class CreateCategoryRequest
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public ForumId ForumId { get; set; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public CategoryTitle Title { get; set; }
}