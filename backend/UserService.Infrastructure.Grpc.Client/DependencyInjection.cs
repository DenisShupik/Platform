using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.ClientFactory;
using UserService.Infrastructure.Grpc.Contracts;
using ZiggyCreatures.Caching.Fusion;

namespace UserService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterUserServiceGrpcClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddFusionCache(Constants.CacheName)
            .WithDefaultEntryOptions(opt => { opt.Duration = TimeSpan.FromSeconds(20); })
            .WithCacheKeyPrefixByCacheName()
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        builder.Services.AddCodeFirstGrpcClient<IGrpcUserService>(options =>
        {
            // TODO: сделать через appsettings или discovery
            options.Address = new Uri("http://localhost:8021");
        });

        builder.Services.AddSingleton<UserServiceGrpcClient>();
    }
}