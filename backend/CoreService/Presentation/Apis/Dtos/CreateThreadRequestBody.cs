using CoreService.Domain.ValueObjects;

namespace CoreService.Presentation.Apis.Dtos;

public sealed class CreateThreadRequestBody
{
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public required CategoryId CategoryId { get; init; }

    /// <summary>
    /// Название темы
    /// </summary>
    public required ThreadTitle Title { get; init; }
}