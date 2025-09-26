using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetThreadsResponse> GetThreadsAsync(GetThreadsRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var query = new GetThreadsBulkQuery<ThreadDto>
        {
            ThreadIds = request.ThreadIds
        };

        var handler = httpContext.RequestServices.GetRequiredService<GetThreadsBulkQueryHandler<ThreadDto>>();
        var response = await handler.HandleAsync(query, cancellationToken);

        return new GetThreadsResponse
        {
            Threads = response.Adapt<GetThreadResponse[]>()
        };
    }
}