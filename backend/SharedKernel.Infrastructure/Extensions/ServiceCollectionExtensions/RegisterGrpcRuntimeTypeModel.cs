using Microsoft.Extensions.DependencyInjection;
using ProtoBuf.Grpc.Configuration;
using ProtoBuf.Meta;

namespace SharedKernel.Infrastructure.Extensions.ServiceCollectionExtensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterGrpcRuntimeTypeModel(this IServiceCollection services,
        Action<RuntimeTypeModel> configure)
    {
        var model = RuntimeTypeModel.Create();
        configure(model);
        var binderConfiguration = BinderConfiguration.Create([
            ProtoBufMarshallerFactory.Create(model, ProtoBufMarshallerFactory.Options.None)
        ]);
        services.AddSingleton(binderConfiguration);
        return services;
    }
}