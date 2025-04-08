using CoreService.Domain.Abstractions;

namespace CoreService.Application.Dtos;

public sealed class ForumDto : IHasCreatedProperties
{ 
    /// <summary>
    /// Идентификатор раздела
    /// </summary>
    public long ForumId { get; set; }

    /// <summary>
    /// Наименование раздела
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Дата и время создания раздела
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Идентификатор пользователя, создавшего раздел
    /// </summary>
    public Guid CreatedBy { get; set; }
}