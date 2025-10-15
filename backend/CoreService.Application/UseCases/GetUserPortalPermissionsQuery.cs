using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using Shared.Application.Interfaces;
using UserService.Domain.ValueObjects;

namespace CoreService.Application.UseCases;

using QueryResult = Dictionary<PolicyType, bool>;

public sealed class GetUserPortalPermissionsQuery : IQuery<QueryResult>
{
    public required UserId? QueriedBy { get; init; }
}

public sealed class GetUserPortalPermissionsQueryHandler : IQueryHandler<GetUserPortalPermissionsQuery, QueryResult>
{
    private readonly IAccessReadRepository _accessReadRepository;

    public GetUserPortalPermissionsQueryHandler(
        IAccessReadRepository accessReadRepository
    )
    {
        _accessReadRepository = accessReadRepository;
    }

    public Task<QueryResult> HandleAsync(GetUserPortalPermissionsQuery query, CancellationToken cancellationToken)
    {
        return _accessReadRepository.GetPortalPermissionsAsync(query, cancellationToken);
    }
}