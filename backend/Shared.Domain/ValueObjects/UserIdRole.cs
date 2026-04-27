using Shared.Domain.Enums;

namespace Shared.Domain.ValueObjects;

public readonly record struct UserIdRole(UserId UserId, Role Role);