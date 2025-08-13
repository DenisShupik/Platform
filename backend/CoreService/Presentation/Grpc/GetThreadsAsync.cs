using CoreService.Application.Dtos;
using CoreService.Application.UseCases;
using CoreService.Domain.ValueObjects;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using SharedKernel.Application.Abstractions;
using Wolverine;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    public async ValueTask<GetThreadsResponse> GetThreadsAsync(GetThreadsRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");

        var query = new GetThreadsBulkQuery
        {
            ThreadIds = IdSet<ThreadId>.Create(request.ThreadIds)
        };

        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<IReadOnlyList<ThreadDto>>(query, cancellationToken);

        return new GetThreadsResponse
        {
            Threads = response.Adapt<GetThreadResponse[]>()
        };
    }
}