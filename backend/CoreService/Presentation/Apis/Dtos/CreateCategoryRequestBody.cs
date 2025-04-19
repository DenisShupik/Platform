using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Apis.Dtos;

public sealed class CreateCategoryRequestBody
{
    /// <summary>
    /// Идентификатор форума
    /// </summary>
    public required ForumId ForumId { get; init; }

    /// <summary>
    /// Название раздела
    /// </summary>
    public required CategoryTitle Title { get; init; }
}