using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.Errors;
using UserService.Infrastructure.Grpc.Contracts;
using Wolverine;
using OneOf;

namespace UserService.Presentation.Grpc;

public sealed partial class GrpcUserService
{
    public async ValueTask<GetUserResponse> GetUserAsync(GetUserRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var command = new GetUserByIdQuery
        {
            UserId = request.UserId
        };
        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<OneOf<UserDto, UserNotFoundError>>(command, cancellationToken);

        return response.Match<GetUserResponse>(
            data => data.Adapt<GetUserResponse>(),
            userNotFound => throw userNotFound.GetRpcException()
        );
    }
}