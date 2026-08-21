using CoreService.Domain.Enums;
using Shared.Domain.Abstractions.Errors;

namespace CoreService.Domain.Errors;

public sealed record AuthorizationScopeNotFoundError(AuthorizationScopeType ScopeType) : NotFoundError;
