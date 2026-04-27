using CoreService.Domain.ValueObjects;
using ProtoBuf.Meta;
using Shared.Domain.ValueObjects;
using Shared.Infrastructure.Grpc;

namespace CoreService.Infrastructure.Grpc.Contracts;

public static class RuntimeTypeModelExtensions
{
    public static void MapCoreServiceTypes(this RuntimeTypeModel model)
    {
        GuidIdSerializer<ForumId>.Configure(model);
        GuidIdSerializer<CategoryId>.Configure(model);
        GuidIdSerializer<ThreadId>.Configure(model);
        GuidIdSerializer<PostId>.Configure(model);
        GuidIdSerializer<UserId>.Configure(model);
        model.Add<PostContent>(false).SetSurrogate(typeof(string));
        model.Add<ThreadTitle>(false).SetSurrogate(typeof(string));
        model.Add<Count>(false).SetSurrogate(typeof(int));
    }
}