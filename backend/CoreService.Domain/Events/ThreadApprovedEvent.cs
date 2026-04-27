using Shared.Domain.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Domain.Events;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId), nameof(Thread.CreatedBy))]
public sealed partial class ThreadApprovedEvent : IDomainEvent
{
    /// <summary>
    /// Идентификатор модератора
    /// </summary>
    public required UserId ApprovedBy { get; init; }

    /// <summary>
    /// Дата и время, когда тема прошла модерацию
    /// </summary>
    public required DateTime ApprovedAt { get; init; }
}