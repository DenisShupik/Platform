using NotificationService.Application.Interfaces;
using Shared.Application.Abstractions;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.Domain.Enums;
using Shared.Domain.Errors;
using Shared.Domain.ValueObjects;

namespace NotificationService.Application.UseCases;

public enum GetThreadSubscriptionLatestEventsPagedQuerySortType : byte
{
    LatestEvent = 0,
    ThreadId = 1
}

public sealed class GetThreadSubscriptionLatestEventsPagedQuery<T> : SingleSortPagedQuery<
    Result<IReadOnlyList<T>, NotAdminError>,
    GetThreadSubscriptionLatestEventsPagedQuerySortType
>
{
    public required UserId UserId { get; init; }
    public required UserIdRole RequestedBy { get; init; }
}

public sealed class GetThreadSubscriptionLatestEventsPagedQueryHandler<T> : IQueryHandler<
    GetThreadSubscriptionLatestEventsPagedQuery<T>,
    Result<IReadOnlyList<T>, NotAdminError>
>
{
    private readonly IThreadSubscriptionReadRepository _repository;

    public GetThreadSubscriptionLatestEventsPagedQueryHandler(IThreadSubscriptionReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<T>, NotAdminError>> HandleAsync(
        GetThreadSubscriptionLatestEventsPagedQuery<T> query,
        CancellationToken cancellationToken
    )
    {
        if (query.UserId != query.RequestedBy.UserId && query.RequestedBy.Role != Role.Administrator)
            return new NotAdminError();

        var events = await _repository.GetLatestEventsAsync(
            query,
            cancellationToken
        );

        return events;
    }
}
