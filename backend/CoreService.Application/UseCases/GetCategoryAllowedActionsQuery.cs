using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryAllowedActionsQuery : IQuery<
    Result<CategoryAllowedActionsDto, CategoryNotFoundError>>
{
    public required CategoryId CategoryId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetCategoryAllowedActionsQueryHandler(
    ICategoryReadRepository categories,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetCategoryAllowedActionsQuery,
    Result<CategoryAllowedActionsDto, CategoryNotFoundError>>
{
    public async Task<Result<CategoryAllowedActionsDto, CategoryNotFoundError>> HandleAsync(
        GetCategoryAllowedActionsQuery query,
        CancellationToken cancellationToken)
    {
        var scopeResult = await categories.GetAuthorizationScopeAsync(query.CategoryId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var categoryError)) return categoryError;

        var allowed = await policies.GetAllowedAsync(
            query.RequestedBy,
            scope,
            query.EvaluatedAt,
            cancellationToken);

        return new CategoryAllowedActionsDto
        {
            CanManageStructure = allowed.Contains(ForumPolicy.ManageStructure),
            CanViewUnpublishedThreads = allowed.Contains(ForumPolicy.ViewUnpublishedThreads),
            CanApproveThread = allowed.Contains(ForumPolicy.ApproveThread),
            CanRejectThread = allowed.Contains(ForumPolicy.RejectThread),
            CanEditAnyPost = allowed.Contains(ForumPolicy.EditAnyPost),
            CanDeleteAnyPost = allowed.Contains(ForumPolicy.DeleteAnyPost),
            CanManageModerators = allowed.Contains(ForumPolicy.ManageAuthorization),
            CanManageSanctions = allowed.Contains(ForumPolicy.ManageSanctions)
        };
    }
}
