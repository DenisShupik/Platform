using CoreService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class CreateThreadRequest
{
    /// <summary>
    /// Идентификатор категории
    /// </summary>
    public CategoryId CategoryId { get; set; }

    /// <summary>
    /// Название темы
    /// </summary>
    public ThreadTitle Title { get; set; }
}