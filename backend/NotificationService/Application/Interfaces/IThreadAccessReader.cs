using CoreService.Domain.ValueObjects;
using NotificationService.Application.Dtos;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.Interfaces;

public interface IThreadAccessReader
{
    ValueTask<bool> CanReadAsync(
        ThreadId threadId,
        UserId actorId,
        CancellationToken cancellationToken);

    ValueTask<IReadOnlyList<ThreadSummaryDto>> GetReadableAsync(
        IReadOnlySet<ThreadId> threadIds,
        UserId actorId,
        CancellationToken cancellationToken);
}
