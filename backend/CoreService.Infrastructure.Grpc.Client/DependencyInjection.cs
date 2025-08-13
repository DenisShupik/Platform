using CoreService.Application.Interfaces;
using CoreService.Infrastructure.Cache;
using CoreService.Infrastructure.Grpc.Contracts;
using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.ClientFactory;
using ProtoBuf.Meta;

namespace CoreService.Infrastructure.Grpc.Client;

public static class DependencyInjection
{
    public static void RegisterCoreServiceGrpcClient(this IServiceCollection services, RuntimeTypeModel model)
    {
        services.RegisterCoreServiceCache(options =>
        {
            options.Duration = TimeSpan.FromMinutes(5);
            options.DistributedCacheDuration = TimeSpan.FromHours(1);
        });

        model.MapCoreServiceTypes();
        services.AddCodeFirstGrpcClient<IGrpcCoreService>(options =>
        {
            // TODO: сделать через appsettings или discovery
            options.Address = new Uri("http://localhost:8011");
        });

        services.AddSingleton<ICoreServiceClient, CoreServiceGrpcClient>();
    }
}