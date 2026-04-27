using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Infrastructure.Grpc.Contracts;

namespace UserService.Presentation.Grpc;

public sealed partial class GrpcUserService
{
    public async ValueTask<GetUserResponse> GetUserAsync(GetUserRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetUserQuery<UserDto>
        {
            UserId = request.UserId
        };
        var handler = httpContext.RequestServices.GetRequiredService<GetUserQueryHandler<UserDto>>();
        var response = await handler.HandleAsync(command, cancellationToken);

        return response.Match<GetUserResponse>(
            data => data.Adapt<GetUserResponse>(),
            userNotFound => throw userNotFound.GetRpcException()
        );
    }
}