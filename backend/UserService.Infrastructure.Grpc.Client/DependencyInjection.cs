using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.ClientFactory;
using ProtoBuf.Meta;
using UserService.Application.Interfaces;
using UserService.Infrastructure.Cache;
using UserService.Infrastructure.Grpc.Contracts;

namespace UserService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterUserServiceGrpcClient(this IHostApplicationBuilder builder)
    {
        builder.RegisterUserServiceCache(options =>
        {
            options.Duration = TimeSpan.FromMinutes(5);
            options.DistributedCacheDuration = TimeSpan.FromHours(1);
        });

        RuntimeTypeModel.Default.MapUserServiceTypes();
        builder.Services.AddCodeFirstGrpcClient<IGrpcUserService>(options =>
        {
            // TODO: сделать через appsettings или discovery
            options.Address = new Uri("http://localhost:8021");
        });

        builder.Services.AddSingleton<IUserServiceClient, UserServiceGrpcClient>();
    }
}