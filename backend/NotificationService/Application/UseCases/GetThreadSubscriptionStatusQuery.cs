using NotificationService.Application.Authorization;
using NotificationService.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.ThreadId))]
public sealed partial class
    GetThreadSubscriptionStatusQuery : IQuery<Result<GetThreadSubscriptionStatusQueryResult, PermissionDeniedError>>
{
    public required UserId UserId { get; init; }
    public required ActorContext RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionStatusQueryResult
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/member[@key='GetThreadSubscriptionStatusQueryResult.IsSubscribed']/*" />
    public required bool IsSubscribed { get; init; }
}

public sealed class GetThreadSubscriptionStatusQueryHandler : IQueryHandler<
    GetThreadSubscriptionStatusQuery,
    Result<GetThreadSubscriptionStatusQueryResult, PermissionDeniedError>
>
{
    private readonly IThreadSubscriptionReadRepository _threadSubscriptionReadRepository;
    private readonly IThreadSubscriptionPolicyEvaluator _policyEvaluator;

    public GetThreadSubscriptionStatusQueryHandler(
        IThreadSubscriptionReadRepository threadSubscriptionReadRepository,
        IThreadSubscriptionPolicyEvaluator policyEvaluator
    )
    {
        _threadSubscriptionReadRepository = threadSubscriptionReadRepository;
        _policyEvaluator = policyEvaluator;
    }

    public async Task<Result<GetThreadSubscriptionStatusQueryResult, PermissionDeniedError>> HandleAsync(
        GetThreadSubscriptionStatusQuery query,
        CancellationToken cancellationToken
    )
    {
        var authorization = _policyEvaluator.Authorize(
            query.RequestedBy,
            ThreadSubscriptionPolicy.Read,
            query.UserId);
        if (authorization.TryGetFailure(out var authorizationFailure)) return authorizationFailure;

        var isSubscribed = await _threadSubscriptionReadRepository.ExistsAsync(
            query.UserId,
            query.ThreadId,
            cancellationToken
        );

        return new GetThreadSubscriptionStatusQueryResult { IsSubscribed = isSubscribed };
    }
}
