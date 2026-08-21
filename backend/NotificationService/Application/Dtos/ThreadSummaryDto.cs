using CoreService.Domain.ValueObjects;
using Shared.Domain.ValueObjects;
using ForumThreadState = CoreService.Domain.Enums.ThreadState;

namespace NotificationService.Application.Dtos;

public sealed class ThreadSummaryDto
{
    public required ThreadId ThreadId { get; init; }
    public required CategoryId CategoryId { get; init; }
    public required ThreadTitle Title { get; init; }
    public required UserId CreatedBy { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required ForumThreadState State { get; init; }
    public required Count PostCount { get; init; }
    public required PostId? LastHeaderPostId { get; init; }
}
