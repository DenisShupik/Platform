using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetThreadAllowedActionsQuery : IQuery<Result<ThreadAllowedActionsDto, ThreadNotFoundError>>
{
    public required ThreadId ThreadId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetThreadAllowedActionsQueryHandler(
    IThreadReadRepository threads,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetThreadAllowedActionsQuery,
    Result<ThreadAllowedActionsDto, ThreadNotFoundError>>
{
    public async Task<Result<ThreadAllowedActionsDto, ThreadNotFoundError>> HandleAsync(
        GetThreadAllowedActionsQuery query,
        CancellationToken cancellationToken)
    {
        var scopeResult = await threads.GetAuthorizationScopeAsync(query.ThreadId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var threadError)) return threadError;

        var allowed = await policies.GetAllowedAsync(
            query.RequestedBy,
            scope,
            query.EvaluatedAt,
            cancellationToken);

        return new ThreadAllowedActionsDto
        {
            CanViewUnpublishedThreads = allowed.Contains(ForumPolicy.ViewUnpublishedThreads),
            CanApproveThread = allowed.Contains(ForumPolicy.ApproveThread),
            CanRejectThread = allowed.Contains(ForumPolicy.RejectThread),
            CanEditAnyPost = allowed.Contains(ForumPolicy.EditAnyPost),
            CanDeleteAnyPost = allowed.Contains(ForumPolicy.DeleteAnyPost),
            CanManageAuthorization = allowed.Contains(ForumPolicy.ManageAuthorization),
            CanManageSanctions = allowed.Contains(ForumPolicy.ManageSanctions)
        };
    }
}
