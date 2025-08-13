using ProtoBuf;
using UserService.Domain.ValueObjects;

namespace UserService.Infrastructure.Grpc.Contracts;

[ProtoContract]
public sealed class GetUserRequest
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [ProtoMember(1)]
    public UserId UserId { get; set; }
}

[ProtoContract]
public sealed class GetUserResponse
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [ProtoMember(1)]
    public UserId UserId { get; set; }

    /// <summary>
    /// Логин пользователя
    /// </summary>
    [ProtoMember(2)]
    public Username Username { get; set; }

    /// <summary>
    /// Электронная почта пользователя
    /// </summary>
    [ProtoMember(3)]
    public string Email { get; set; }

    /// <summary>
    /// Активна ли учетная запись пользователя
    /// </summary>
    [ProtoMember(4)]
    public bool Enabled { get; set; }

    /// <summary>
    /// Дата и время создания учетной записи пользователя
    /// </summary>
    [ProtoMember(5)]
    public DateTime CreatedAt { get; set; }
}