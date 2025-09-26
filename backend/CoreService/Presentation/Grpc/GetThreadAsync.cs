using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Domain.Enums;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetThreadResponse> GetThreadAsync(GetThreadRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetThreadQuery<ThreadDto>
        {
            ThreadId = request.ThreadId,
            Role = RoleType.Service,
            QueriedBy = null
        };
        var handler = httpContext.RequestServices.GetRequiredService<GetThreadQueryHandler<ThreadDto>>();
        var response = await handler.HandleAsync(command, cancellationToken);


        var t = response.Match<GetThreadResponse>(
            data => data.Adapt<GetThreadResponse>(),
            threadNotFound => throw threadNotFound.GetRpcException(),
            nonThreadOwner => throw nonThreadOwner.GetRpcException()
        );
        return t;
    }
}