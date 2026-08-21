using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Domain.Abstractions.Results;

namespace CoreService.Application.Authorization;

public sealed class AuthorizationScopeResolver(
    IForumReadRepository forums,
    ICategoryReadRepository categories,
    IThreadReadRepository threads) : IAuthorizationScopeResolver
{
    public async Task<Result<AuthorizationScope, AuthorizationScopeNotFoundError, InvalidAuthorizationScopeError>>
        ResolveAsync(
            AuthorizationScopeType scopeType,
            ForumId? forumId,
            CategoryId? categoryId,
            ThreadId? threadId,
            CancellationToken cancellationToken)
    {
        switch (scopeType)
        {
            case AuthorizationScopeType.Platform when forumId is null && categoryId is null && threadId is null:
                return AuthorizationScope.Platform;
            case AuthorizationScopeType.Forum when forumId is { } requestedForumId && categoryId is null &&
                threadId is null:
            {
                var result = await forums.GetAuthorizationScopeAsync(requestedForumId, cancellationToken);
                return result.TryGetValue(out var scope, out _)
                    ? scope
                    : new AuthorizationScopeNotFoundError(scopeType);
            }
            case AuthorizationScopeType.Category when forumId is null && categoryId is { } requestedCategoryId &&
                threadId is null:
            {
                var result = await categories.GetAuthorizationScopeAsync(requestedCategoryId, cancellationToken);
                return result.TryGetValue(out var scope, out _)
                    ? scope
                    : new AuthorizationScopeNotFoundError(scopeType);
            }
            case AuthorizationScopeType.Thread when forumId is null && categoryId is null &&
                threadId is { } requestedThreadId:
            {
                var result = await threads.GetAuthorizationScopeAsync(requestedThreadId, cancellationToken);
                return result.TryGetValue(out var scope, out _)
                    ? scope
                    : new AuthorizationScopeNotFoundError(scopeType);
            }
            default:
                return new InvalidAuthorizationScopeError();
        }
    }
}
