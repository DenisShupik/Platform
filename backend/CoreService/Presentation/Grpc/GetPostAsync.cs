using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetPostQuery<PostDto>
        {
            PostId = request.PostId,
            // TODO: для сервисных учеток нужно придумать как быть
            QueriedBy = null
        };
        var handler = httpContext.RequestServices.GetRequiredService<GetPostQueryHandler<PostDto>>();
        var response = await handler.HandleAsync(command, cancellationToken);

        return response.Match<GetPostResponse>(
            data => data.Adapt<GetPostResponse>(),
            accessLevelError => throw accessLevelError.GetRpcException(),
            accessRestrictedError => throw accessRestrictedError.GetRpcException(),
            postNotFound => throw postNotFound.GetRpcException()
        );
    }
}