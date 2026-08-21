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
    public async ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var queriedBy = await ResolveActorAsync(request.RequestedBy, httpContext.RequestServices, cancellationToken);
        var command = new GetPostQuery<PostDto>
        {
            PostId = request.PostId,
            QueriedBy = queriedBy
        };

        var handler = httpContext.RequestServices.GetRequiredService<GetPostQueryHandler<PostDto>>();
        var response = await handler.HandleAsync(command, cancellationToken);

        return response.Match<GetPostResponse>(
            value => value.Adapt<GetPostResponse>(),
            error => throw error.GetRpcException(),
            error => throw error.GetRpcException()
        );
    }
}
