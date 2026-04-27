using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using ProtoBuf.Grpc;
using Shared.Presentation.Extensions;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetThreadsResponse> GetThreadsAsync(GetThreadsRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var queriedBy = httpContext.GetRequiredUserIdRole();
        var query = new GetThreadsBulkQuery<GetThreadResponse>
        {
            ThreadIds = request.ThreadIds,
            QueriedBy = queriedBy,
        };

        var handler = httpContext.RequestServices.GetRequiredService<GetThreadsBulkQueryHandler<GetThreadResponse>>();
        var result = await handler.HandleAsync(query, cancellationToken);

        var threads = new List<GetThreadResponse>(result.Capacity);
        foreach (var item in result.Values)
        {
            if (!item.GetValue(out var threadDto)) continue;
            threads.Add(threadDto);
        }

        return new GetThreadsResponse
        {
            Threads = threads
        };
    }
}