using ProtoBuf.Meta;
using SharedKernel.Infrastructure.Grpc;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Grpc.Contracts;

public static class RuntimeTypeModelExtensions
{
    public static void MapUserServiceTypes(this RuntimeTypeModel model)
    {
        GuidIdSerializer<UserId>.Configure(model);
        model.Add<Username>(false).SetSurrogate(typeof(string));
    }
}