using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Client;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using NotificationService.Application.Dtos;
using NotificationService.Application.Interfaces;
using Shared.Domain.Abstractions;
using Shared.Domain.ValueObjects;

namespace NotificationService.Infrastructure.Clients;

public sealed class CoreThreadAccessReader(CoreServiceGrpcClient coreService) : IThreadAccessReader
{
    public async ValueTask<bool> CanReadAsync(
        ThreadId threadId,
        UserId actorId,
        CancellationToken cancellationToken)
    {
        try
        {
            await coreService.GetThreadAsync(new GetThreadRequest
            {
                ThreadId = threadId,
                RequestedBy = new RequestedByActor { UserId = actorId }
            }, cancellationToken);
            return true;
        }
        catch (RpcException exception) when (exception.StatusCode is StatusCode.NotFound or StatusCode.PermissionDenied)
        {
            return false;
        }
    }

    public async ValueTask<IReadOnlyList<ThreadSummaryDto>> GetReadableAsync(
        IReadOnlySet<ThreadId> threadIds,
        UserId actorId,
        CancellationToken cancellationToken)
    {
        if (threadIds.Count == 0) return [];

        var response = await coreService.GetThreadsAsync(new GetThreadsRequest
        {
            ThreadIds = new IdSet<ThreadId, Guid>(threadIds.ToHashSet()),
            RequestedBy = new RequestedByActor { UserId = actorId }
        }, cancellationToken);

        return response.Threads.Select(Map).ToList();
    }

    private static ThreadSummaryDto Map(GetThreadResponse thread) => new()
    {
        ThreadId = thread.ThreadId,
        CategoryId = thread.CategoryId,
        Title = thread.Title,
        CreatedBy = thread.CreatedBy
                    ?? throw new InvalidOperationException("CoreService returned a thread without its creator."),
        CreatedAt = thread.CreatedAt,
        State = thread.State,
        PostCount = thread.PostCount,
        LastHeaderPostId = null
    };
}
