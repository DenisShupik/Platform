using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Grpc.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.ClientFactory;
using ZiggyCreatures.Caching.Fusion;

namespace CoreService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterCoreServiceGrpcClient(this IHostApplicationBuilder builder)
    {
        builder.Services.AddFusionCache(Constants.CacheName)
            .WithDefaultEntryOptions(opt => { opt.Duration = TimeSpan.FromSeconds(20); })
            .WithCacheKeyPrefixByCacheName()
            .WithRegisteredSerializer()
            .WithRegisteredDistributedCache()
            .WithRegisteredBackplane();

        builder.Services.AddCodeFirstGrpcClient<IGrpcCoreService>(options =>
        {
            // TODO: сделать через appsettings или discovery
            options.Address = new Uri("http://localhost:8011");
        });

        builder.Services.AddSingleton<ICoreServiceClient, CoreServiceGrpcClient>();
    }
}