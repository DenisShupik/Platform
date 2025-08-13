using Grpc.Core;
using Mapster;
using ProtoBuf.Grpc;
using SharedKernel.Application.Abstractions;
using UserService.Application.Dtos;
using UserService.Application.UseCases;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Grpc.Contracts;
using Wolverine;

namespace UserService.Presentation.Grpc;

public sealed partial class GrpcUserService
{
    public async ValueTask<GetUsersResponse> GetUsersAsync(GetUsersRequest request, CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var query = new GetUsersBulkQuery
        {
            UserIds = IdSet<UserId>.Create(request.UserIds)
        };
        var messageBus = httpContext.RequestServices.GetRequiredService<IMessageBus>();
        var response = await messageBus.InvokeAsync<IReadOnlyList<UserDto>>(query, cancellationToken);

        return new GetUsersResponse
        {
            Users = response.Adapt<GetUserResponse[]>()
        };
    }
}