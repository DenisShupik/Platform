using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.ClientFactory;
using ProtoBuf.Meta;
using Shared.Infrastructure.Services;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Grpc.Contracts;

namespace UserService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterUserServiceGrpcClient(this IServiceCollection services, RuntimeTypeModel model)
    {
        services.RegisterUserServiceCache(options =>
        {
            options.Duration = TimeSpan.FromMinutes(5);
            options.DistributedCacheDuration = TimeSpan.FromHours(1);
        });

        services.AddSingleton<ServiceTokenService>();
        services.AddTransient<ServiceTokenService.Handler>();

        model.MapUserServiceTypes();
        services.AddCodeFirstGrpcClient<IGrpcUserService>(options =>
            {
                // TODO: сделать через appsettings или discovery
                options.Address = new Uri("http://localhost:8021");
            })
            .AddHttpMessageHandler<ServiceTokenService.Handler>();

        services.AddSingleton<IUserServiceClient, UserServiceGrpcClient>();
    }
}