namespace Shared.Domain.ValueObjects;

/// <summary>
/// Идентичность пользователя, от имени которого выполняется UseCase.
/// Полномочия не являются частью контекста и разрешаются политиками владеющего bounded context.
/// </summary>
public readonly record struct ActorContext(UserId UserId);
