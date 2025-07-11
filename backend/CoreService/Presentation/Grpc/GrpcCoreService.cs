using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using CoreService.Domain.ValueObjects;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Domain.Enums;
using Wolverine;

namespace CoreService.Presentation.Grpc;

public sealed class GrpcCoreService : IGrpcCoreService
{
    public async ValueTask<GetThreadResponse> GetThreadAsync(GetThreadRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetThreadQuery
        {
            ThreadId = ThreadId.From(request.ThreadId),
            Role = RoleType.Service,
            QueriedBy = null
        };
        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<GetThreadQueryResult<ThreadDto>>(command, cancellationToken);


       
        var t= response.Match<GetThreadResponse>(
            data => data.Adapt<GetThreadResponse>(),
            threadNotFound => throw threadNotFound.GetRpcException(),
            nonThreadOwner => throw nonThreadOwner.GetRpcException()
        );
        return t;
    }

    public async ValueTask<GetPostResponse> GetPostAsync(GetPostRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetPostQuery
        {
            ThreadId = ThreadId.From(request.ThreadId),
            PostId = PostId.From(request.PostId)
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