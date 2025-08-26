using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Domain.Enums;
using Wolverine;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetThreadResponse> GetThreadAsync(GetThreadRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetThreadQuery
        {
            ThreadId = request.ThreadId,
            Role = RoleType.Service,
            QueriedBy = null
        };
        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<GetThreadQueryResult<ThreadDto>>(command, cancellationToken);


        var t = response.Match<GetThreadResponse>(
            data => data.Adapt<GetThreadResponse>(),
            threadNotFound => throw threadNotFound.GetRpcException(),
            nonThreadOwner => throw nonThreadOwner.GetRpcException()
        );
        return t;
    }
}