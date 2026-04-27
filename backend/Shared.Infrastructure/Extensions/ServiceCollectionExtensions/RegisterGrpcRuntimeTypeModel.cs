using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Meta;

namespace Shared.Infrastructure.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterGrpcRuntimeTypeModel(this IServiceCollection services,
        Action<RuntimeTypeModel> configure)
    {
        var model = RuntimeTypeModel.Create();
        var binderConfiguration = BinderConfiguration.Create([
            ProtoBufMarshallerFactory.Create(model, ProtoBufMarshallerFactory.Options.None)
        ]);
        var clientFactory = ClientFactory.Create(binderConfiguration);
        services.AddSingleton(binderConfiguration);
        services.AddSingleton(clientFactory);
        configure(model);
        return services;
    }
}