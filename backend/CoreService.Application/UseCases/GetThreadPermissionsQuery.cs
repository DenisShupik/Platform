using CoreService.Application.Interfaces;
using CoreService.Domain.Enums;
using CoreService.Domain.Errors;
using Shared.Application.Interfaces;
using Shared.Domain.Abstractions.Results;
using Shared.TypeGenerator.Attributes;
using UserService.Domain.ValueObjects;
using Thread = CoreService.Domain.Entities.Thread;

namespace CoreService.Application.UseCases;

using QueryResult = Result<
    Dictionary<PolicyType, bool>,
    ThreadNotFoundError
>;

[Include(typeof(Thread), PropertyGenerationMode.AsRequired, nameof(Thread.ThreadId))]
public sealed partial class GetThreadPermissionsQuery : IQuery<QueryResult>
{
    public required UserId? QueriedBy { get; init; }
    public required DateTime EvaluatedAt { get; init; }
}

public sealed class GetThreadPermissionsQueryHandler : IQueryHandler<GetThreadPermissionsQuery, QueryResult>
{
    private readonly IAccessReadRepository _accessReadRepository;

    public GetThreadPermissionsQueryHandler(
        IAccessReadRepository accessReadRepository
    )
    {
        _accessReadRepository = accessReadRepository;
    }

    public Task<QueryResult> HandleAsync(GetThreadPermissionsQuery query, CancellationToken cancellationToken)
    {
        return _accessReadRepository.GetThreadPermissionsAsync(query, cancellationToken);
    }
}