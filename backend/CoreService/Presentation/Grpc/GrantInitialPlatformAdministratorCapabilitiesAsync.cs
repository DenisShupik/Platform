using CoreService.Application.UseCases;
using CoreService.Infrastructure.Grpc.Contracts;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ProtoBuf.Grpc;
using Shared.Presentation.Authorization;
using Shared.Domain.Errors;
using UserService.Infrastructure.Grpc.Contracts;

namespace CoreService.Presentation.Grpc;

public sealed partial class GrpcCoreService
{
    [Authorize(Policy = AuthenticationPolicies.ProvisioningServiceInternalApi)]
    public async ValueTask<GrantInitialPlatformAdministratorCapabilitiesResponse>
        GrantInitialPlatformAdministratorCapabilitiesAsync(
            GrantInitialPlatformAdministratorCapabilitiesRequest request,
            CallContext context = default)
    {
        var cancellationToken = context.CancellationToken;
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new Exception("Internal server error");
        var handler = httpContext.RequestServices
            .GetRequiredService<BootstrapPlatformAdministratorsCommandHandler>();

        var result = await handler.HandleAsync(
            new BootstrapPlatformAdministratorsCommand
            {
                UserIds = [request.UserId],
                BootstrappedAt = DateTime.UtcNow
            },
            cancellationToken);
        if (result.TryGetFailure(out var failure) &&
            failure.TryGet<UserNotFoundError>(out var userNotFound))
            throw userNotFound.GetRpcException();

        return new GrantInitialPlatformAdministratorCapabilitiesResponse();
    }
}
