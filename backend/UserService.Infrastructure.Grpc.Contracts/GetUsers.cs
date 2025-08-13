using ProtoBuf;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetUsersRequest
{
    /// <summary>
    /// Идентификаторы пользователей
    /// </summary>
    [ProtoMember(1)]
    public required HashSet<UserId> UserIds { get; init; }
}

[ProtoContract]
public sealed class GetUsersResponse
{
    /// <summary>
    /// Пользователи
    /// </summary>
    [ProtoMember(1)]
    public required GetUserResponse[] Users { get; init; }
}