using System.Collections.Concurrent;
using CoreService.Domain.ValueObjects;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using Shared.Domain.ValueObjects;

namespace IntegrationTests.TestDoubles;

public sealed class TestThreadAccessReader : IThreadAccessReader
{
    private readonly ConcurrentDictionary<ThreadId, byte> _deniedThreadIds = new();

    public void Deny(ThreadId threadId) => _deniedThreadIds.TryAdd(threadId, 0);

    public ValueTask<bool> CanReadAsync(
        ThreadId threadId,
        UserId actorId,
        CancellationToken cancellationToken) =>
        ValueTask.FromResult(!_deniedThreadIds.ContainsKey(threadId));

    public ValueTask<IReadOnlyList<ThreadSummaryDto>> GetReadableAsync(
        IReadOnlySet<ThreadId> threadIds,
        UserId actorId,
        CancellationToken cancellationToken) =>
        ValueTask.FromResult<IReadOnlyList<ThreadSummaryDto>>([]);
}
