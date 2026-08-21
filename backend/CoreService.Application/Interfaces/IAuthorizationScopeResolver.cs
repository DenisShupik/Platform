using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Interfaces;

public interface IAuthorizationScopeResolver
{
    Task<Result<AuthorizationScope, AuthorizationScopeNotFoundError, InvalidAuthorizationScopeError>>
        ResolveAsync(
            AuthorizationScopeType scopeType,
            ForumId? forumId,
            CategoryId? categoryId,
            ThreadId? threadId,
            CancellationToken cancellationToken);
}
