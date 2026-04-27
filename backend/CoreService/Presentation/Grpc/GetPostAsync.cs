using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var queriedBy = httpContext.GetRequiredUserIdRole();
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