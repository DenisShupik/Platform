using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;
using Shared.TypeGenerator.Attributes;

namespace NotificationService.Application.UseCases;

[Include(typeof(ThreadSubscription), PropertyGenerationMode.AsRequired, nameof(ThreadSubscription.ThreadId))]
public sealed partial class
    GetThreadSubscriptionStatusQuery : IQuery<Result<GetThreadSubscriptionStatusQueryResult, NotAdminError>>
{
    public required UserId UserId { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionStatusQueryResult
{
    /// <include file="../../Documentation/Api.en.xml" path="docs/member[@key='GetThreadSubscriptionStatusQueryResult.IsSubscribed']/*" />
    public required bool IsSubscribed { get; init; }
}

public sealed class GetThreadSubscriptionStatusQueryHandler : IQueryHandler<
    GetThreadSubscriptionStatusQuery,
    Result<GetThreadSubscriptionStatusQueryResult, NotAdminError>
>
{
    private readonly IThreadSubscriptionReadRepository _threadSubscriptionReadRepository;

    public GetThreadSubscriptionStatusQueryHandler(
        IThreadSubscriptionReadRepository threadSubscriptionReadRepository
    )
    {
        _threadSubscriptionReadRepository = threadSubscriptionReadRepository;
    }

    public async Task<Result<GetThreadSubscriptionStatusQueryResult, NotAdminError>> HandleAsync(
        GetThreadSubscriptionStatusQuery query,
        CancellationToken cancellationToken
    )
    {
        if (query.UserId != query.RequestedBy.UserId && query.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var isSubscribed = await _threadSubscriptionReadRepository.ExistsAsync(
            query.UserId,
            query.ThreadId,
            cancellationToken
        );

        return new GetThreadSubscriptionStatusQueryResult { IsSubscribed = isSubscribed };
    }
}
