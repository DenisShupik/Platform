using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Grpc.ClientFactory;

namespace CoreService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterCoreServiceGrpcClient(this IHostApplicationBuilder builder)
    {
        builder.RegisterCoreServiceCache(options =>
        {
            options.Duration = TimeSpan.FromMinutes(5);
            options.DistributedCacheDuration = TimeSpan.FromHours(1);
        });

        builder.Services.AddCodeFirstGrpcClient<IGrpcCoreService>(options =>
        {
            // TODO: сделать через appsettings или discovery
            options.Address = new Uri("http://localhost:8011");
        });

        builder.Services.AddSingleton<ICoreServiceClient, CoreServiceGrpcClient>();
    }
}