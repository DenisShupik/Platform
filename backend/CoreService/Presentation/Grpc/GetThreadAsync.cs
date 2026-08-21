using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using ProtoBuf.Grpc;
using Shared.Presentation.Authorization;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    [Authorize(Policy = AuthenticationPolicies.NotificationServiceInternalApi)]
    public async ValueTask<GetThreadResponse> GetThreadAsync(GetThreadRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var queriedBy = await ResolveActorAsync(request.RequestedBy, httpContext.RequestServices, cancellationToken);
        var command = new GetThreadQuery<ThreadDto>
        {
            ThreadId = request.ThreadId,
            QueriedBy = queriedBy
        };

        var handler = httpContext.RequestServices.GetRequiredService<GetThreadQueryHandler<ThreadDto>>();
        var response = await handler.HandleAsync(command, cancellationToken);

        var t = response.Match<GetThreadResponse>(
            value => value.Adapt<GetThreadResponse>(),
            error => throw error.GetRpcException(),
            error => throw error.GetRpcException()
        );
        return t;
    }
}
