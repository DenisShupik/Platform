using CoreService.Infrastructure.Grpc.Contracts;
using Shared.Domain.ValueObjects;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    private static ValueTask<ActorContext?> ResolveActorAsync(
        RequestedByActor? requestedBy,
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        return ValueTask.FromResult<ActorContext?>(
            requestedBy is null ? null : new ActorContext(requestedBy.UserId));
    }
}
