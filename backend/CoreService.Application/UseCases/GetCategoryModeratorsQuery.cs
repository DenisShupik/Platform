using CoreService.Application.Authorization;
using CoreService.Application.Dtos;
using CoreService.Application.Interfaces;
using CoreService.Domain.Errors;
using CoreService.Domain.ValueObjects;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

public sealed class GetCategoryModeratorsQuery : IQuery<
    Result<IReadOnlyList<CategoryModeratorAppointmentDto>, PermissionDeniedError, CategoryNotFoundError>>
{
    public required CategoryId CategoryId { get; init; }
    public required ActorContext RequestedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetCategoryModeratorsQueryHandler(
    ICategoryReadRepository categories,
    ICapabilityGrantRepository grants,
    IForumPolicyEvaluator policies) : IQueryHandler<
    GetCategoryModeratorsQuery,
    Result<IReadOnlyList<CategoryModeratorAppointmentDto>, PermissionDeniedError, CategoryNotFoundError>>
{
    public async Task<Result<IReadOnlyList<CategoryModeratorAppointmentDto>, PermissionDeniedError, CategoryNotFoundError>>
        HandleAsync(GetCategoryModeratorsQuery query, CancellationToken cancellationToken)
    {
        var scopeResult = await categories.GetAuthorizationScopeAsync(query.CategoryId, cancellationToken);
        if (!scopeResult.TryGetValue(out var scope, out var categoryError)) return categoryError;

        var authorization = await policies.AuthorizeAsync(
            query.RequestedBy,
            ForumPolicy.ManageAuthorization,
            scope,
            query.EvaluatedAt,
            cancellationToken);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var appointments = await grants.GetActiveCategoryModeratorAppointmentsAsync(
            query.CategoryId,
            query.EvaluatedAt,
            cancellationToken);

        return appointments.ToList();
    }
}
