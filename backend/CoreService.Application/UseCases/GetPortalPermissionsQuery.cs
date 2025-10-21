using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<PolicyType, bool>;

public sealed class GetPortalPermissionsQuery : IQuery<QueryResult>
{
    public required UserId? QueriedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetPortalPermissionsQueryHandler : IQueryHandler<GetPortalPermissionsQuery, QueryResult>
{
    private readonly IAccessReadRepository _accessReadRepository;

    public GetPortalPermissionsQueryHandler(
        IAccessReadRepository accessReadRepository
    )
    {
        _accessReadRepository = accessReadRepository;
    }

    public Task<QueryResult> HandleAsync(GetPortalPermissionsQuery query, CancellationToken cancellationToken)
    {
        return _accessReadRepository.GetPortalPermissionsAsync(query, cancellationToken);
    }
}