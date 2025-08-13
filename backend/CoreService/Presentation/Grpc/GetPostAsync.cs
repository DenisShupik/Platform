using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using Wolverine;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetPostQuery
        {
            ThreadId = request.ThreadId,
            PostId = request.PostId
        };
        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<GetPostQueryResult<PostDto>>(command, cancellationToken);

        return response.Match<GetPostResponse>(
            data => data.Adapt<GetPostResponse>(),
            threadNotFound => throw threadNotFound.GetRpcException(),
            postNotFound => throw postNotFound.GetRpcException()
        );
    }
}