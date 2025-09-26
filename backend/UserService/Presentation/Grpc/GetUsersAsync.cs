using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Infrastructure.Grpc.Contracts;

namespace UserService.Presentation.Grpc;

public sealed partial class GrpcUserService
{
    public async ValueTask<GetUsersResponse> GetUsersAsync(GetUsersRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var query = new GetUsersBulkQuery<UserDto>
        {
            UserIds = request.UserIds
        };
        var handler = httpContext.RequestServices.GetRequiredService<GetUsersBulkQueryHandler<UserDto>>();
        var response = await handler.HandleAsync(query, cancellationToken);

        return new GetUsersResponse
        {
            Users = response.Adapt<GetUserResponse[]>()
        };
    }
}